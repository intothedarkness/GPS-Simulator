//
//  Created by Richard Zhang (Richard.Rupo.Zhang@gmail.com) on 3/2020
//  Copyright © 2020 Richard Zhang. All rights reserved.
//
using iMobileDevice;
using iMobileDevice.iDevice;
using iMobileDevice.Lockdown;
using iMobileDevice.Service;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GPS_Simulator
{
    class device_utils
    {
        public const string ddi_repo_url = @"https://github.com/intothedarkness/iOSDDIRepo/raw/master/";

        public static string get_ddi_image_url(DeviceModel deviceInfo)
        {
            return ddi_repo_url + deviceInfo.FullVersion + ".zip";
        }

        public static bool is_device_on_dev_mode(string udid)
        {
            bool isDevMode = false;

            LibiMobileDevice.Instance.iDevice.idevice_new(out var device, udid);

            var ret_handshake = LibiMobileDevice.Instance.Lockdown.
                lockdownd_client_new_with_handshake(device, out var client,
                "idevicelocation");

            // Determine if the device is in dev mode by probing 
            // with a service
            var ret_start_service = LibiMobileDevice.Instance.Lockdown.
                lockdownd_start_service(client,
                "com.apple.dt.simulatelocation", out var service2);
            if (ret_start_service == 0)
            {
                isDevMode = true;
            }

            return isDevMode;
        }
    }
    class location_service
    {
        public List<string> DevicesUdid = new List<string>();
        public List<DeviceModel> Devices = new List<DeviceModel>();
        public IiDeviceApi iDevice = LibiMobileDevice.Instance.iDevice;
        public ILockdownApi lockdown = LibiMobileDevice.Instance.Lockdown;
        public IServiceApi service = LibiMobileDevice.Instance.Service;
        private static location_service _instance;

        public Action<string> PrintMessageEvent = null;
        public MainWindow wnd_instance = null;
        private location_service(MainWindow wnd) { wnd_instance = wnd; }
        public static location_service GetInstance(MainWindow wnd) => _instance ?? (_instance = new location_service(wnd));

        private int create_new_device(string udid, ref DeviceModel device)
        {
            iDeviceHandle iDeviceHandle;
            iDevice.idevice_new(out iDeviceHandle, udid).ThrowOnError();
            LockdownClientHandle lockdownClientHandle;

            var ret_handshake = lockdown.lockdownd_client_new_with_handshake(iDeviceHandle,
                out lockdownClientHandle, "Quamotion");
            if (ret_handshake != 0)
            {
                return -1;
            }

            var ret_get_devname = lockdown.lockdownd_get_device_name(lockdownClientHandle,
                out var deviceName);
            if (ret_get_devname != 0)
            {
                return -1;
            }

            ret_handshake = lockdown.lockdownd_client_new_with_handshake(iDeviceHandle,
                out lockdownClientHandle, "waua");
            if (ret_handshake != 0)
            {
                return -1;
            }

            var ret_get_value = lockdown.lockdownd_get_value(lockdownClientHandle, null,
                "ProductVersion", out var node);
            if (ret_get_devname != 0)
            {
                return -1;
            }

            LibiMobileDevice.Instance.Plist.plist_get_string_val(node, out var version);

            ret_get_value = lockdown.lockdownd_get_value(lockdownClientHandle, null,
                "BuildVersion", out node);
            if (ret_get_devname != 0)
            {
                return -1;
            }

            LibiMobileDevice.Instance.Plist.plist_get_string_val(node, out var bldVersion);

            iDeviceHandle.Dispose();
            lockdownClientHandle.Dispose();

            device.UDID = udid;
            device.Name = deviceName;
            device.Version = version;
            device.BuildVersion = bldVersion;
            device.ShortVersion = string.Join(".", version.Split('.').Take(2));
            device.FullVersion = string.Join(".", version.Split('.').Take(2)) + "(" + bldVersion + ")";
            device.isDevMode = device_utils.is_device_on_dev_mode(udid);
            
            return 0;
        }

        public void ListeningDevice()
        {
            var num = 0;
            var deviceError = iDevice.idevice_get_device_list(out var devices, ref num);
            if (deviceError != iDeviceError.Success)
            {
                return;
            }

            ThreadPool.QueueUserWorkItem(o =>
            {
                while (true)
                {
                    deviceError = iDevice.idevice_get_device_list(out devices, ref num);

                    if (devices.Count > 0)
                    {
                        // get the device is newly added
                        var new_devices = devices.Except(Devices.Select(s => s.UDID)).ToList();
                        foreach (string udid in new_devices)
                        {
                            DeviceModel device = new DeviceModel();
                            if(0 == create_new_device(udid, ref device))
                            {
                                device_add_remove(device, dev_op.add_device);
                            }
                        }

                        // remove the device is no longer connected.
                        for (int i = 0; i < Devices.Count; i++)
                        {
                            if (!devices.Contains(Devices[i].UDID))
                            {
                                device_add_remove(Devices[i], dev_op.remove_device);
                            }
                        }
                    }

                    refresh_device_info();
                    Thread.Sleep(1000);
                }
            });
        }

        public enum dev_op
        {
            add_device = 1,
            remove_device = 2,
            clear_all = 3
        }

        public void device_add_remove(DeviceModel device, dev_op op_type)
        {
            switch (op_type)
            {
                case dev_op.add_device:
                    if (device.isDevMode == false)
                    {
                        device.Name += " (UNPROVISIONED)";
                    }
                    Devices.Add(device);
                    break;

                case dev_op.remove_device:
                    Devices.Remove(device);
                    break;

                case dev_op.clear_all:
                    Devices.Clear();
                    break;

                default: break;
            }
        }

        private void refresh_device_info()
        {
            wnd_instance.Dispatcher.Invoke((Action)(() =>
            {
                wnd_instance.device_prov.IsEnabled = (Devices.Count >= 1);

                wnd_instance.connected_dev.Text = "";
                foreach (DeviceModel dev in Devices)
                {
                    wnd_instance.connected_dev.Text += dev.Name;
                    wnd_instance.connected_dev.Text += " is connected.\n";
                }

                if (Devices.Count == 0)
                {
                    wnd_instance.connected_dev.Text = "No iOS device is connected.\n";
                }
            }));
        }

        public void UpdateLocation(string Longitude, string Latitude, string Altitude, DeviceModel itm)
        {
            var size = BitConverter.GetBytes(0u);
            Array.Reverse(size);

            var num = 0u;
            iDevice.idevice_new(out var device, itm.UDID);

            var ret_handshake = lockdown.lockdownd_client_new_with_handshake(device, out var client,
                "devicelocation");

            if (ret_handshake != 0)
            {
                return;
            }

            if (!itm.isDevMode)
            {
                return;
            }

            var ret_start_service = lockdown.lockdownd_start_service(client,
                "com.apple.dt.simulatelocation", out var service2);
            if (ret_start_service != 0)
            {
                return;
            }

            var se = service.service_client_new(device, service2, out var client2);
            if (se != 0)
            {
                return;
            }

            se = service.service_send(client2, size, 4u, ref num);
            if (se != 0)
            {
                return;
            }

            num = 0u;
            var bytesLocation = Encoding.ASCII.GetBytes(Latitude);
            size = BitConverter.GetBytes((uint)Latitude.Length);
            Array.Reverse(size);
            se = service.service_send(client2, size, 4u, ref num);
            se = service.service_send(client2, bytesLocation,
                (uint)bytesLocation.Length, ref num);

            bytesLocation = Encoding.ASCII.GetBytes(Longitude);
            size = BitConverter.GetBytes((uint)Longitude.Length);
            Array.Reverse(size);
            se = service.service_send(client2, size, 4u, ref num);
            se = service.service_send(client2, bytesLocation,
                (uint)bytesLocation.Length, ref num);

            bytesLocation = Encoding.ASCII.GetBytes(Altitude);
            size = BitConverter.GetBytes((uint)Altitude.Length);
            Array.Reverse(size);
            se = service.service_send(client2, size, 4u, ref num);
            se = service.service_send(client2, bytesLocation,
                (uint)bytesLocation.Length, ref num);

            device.Close();
            device.Dispose();
            client.Dispose();
            client2.Dispose();

            service2.Dispose();
        }

        public void UpdateLocation(Location location)
        {
            // no device is connected.
            if (Devices.Count == 0)
            {
                return;
            }

            var Longitude = location.Longitude.ToString();
            var Latitude = location.Latitude.ToString();
            var Altitude = location.Altitude.ToString();

            for (int i = 0; i < Devices.Count(); i++)
            {
                DeviceModel itm = Devices[i];
                UpdateLocation(Longitude, Latitude, Altitude, itm);
            }
        }
    }

    public class DeviceModel
    {
        public string UDID { get; set; }
        public string Version { get; set; } // 13.3.1
        public string Name { get; set; }
        public string BuildVersion { get; set; }  // AXFGGG
        public bool isDevMode { get; set; }
        public string FullVersion { get; set; } // 13.3(AXFGGG)
        public string ShortVersion { get; set; } // 13.3
    }

}

