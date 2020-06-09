//
//  Created by Richard Zhang (Richard.Rupo.Zhang@gmail.com) on 3/2020
//  Copyright © 2020 Richard Zhang. All rights reserved.
//
using iMobileDevice;
using iMobileDevice.iDevice;
using iMobileDevice.Lockdown;
using iMobileDevice.MobileImageMounter;
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

            // determine if the device is in dev mode by probing 
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
        public List<DeviceModel> Devices = new List<DeviceModel>();
        public IiDeviceApi iDevice = LibiMobileDevice.Instance.iDevice;
        public ILockdownApi lockdown = LibiMobileDevice.Instance.Lockdown;
        public IServiceApi service = LibiMobileDevice.Instance.Service;
        public IMobileImageMounterApi mounter = LibiMobileDevice.Instance.MobileImageMounter;
        private static location_service _instance;

        public Action<string> PrintMessageEvent = null;
        public MainWindow wnd_instance = null;
        private location_service(MainWindow wnd) { wnd_instance = wnd; }
        public static location_service GetInstance(MainWindow wnd) => _instance ?? (_instance = new location_service(wnd));

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
                        var lst = Devices.Select(s => s.UDID).ToList().Except(devices).ToList();
                        var dst = devices.Except(Devices.Select(s => s.UDID)).ToList();

                        foreach (string udid in dst)
                        {
                            iDeviceHandle iDeviceHandle;
                            iDevice.idevice_new(out iDeviceHandle, udid).ThrowOnError();
                            LockdownClientHandle lockdownClientHandle;

                            var ret_handshake = lockdown.lockdownd_client_new_with_handshake(iDeviceHandle,
                                out lockdownClientHandle, "Quamotion");
                            if (ret_handshake != 0)
                            {
                                continue;
                            }

                            var ret_get_devname = lockdown.lockdownd_get_device_name(lockdownClientHandle,
                                out var deviceName);
                            if (ret_get_devname != 0)
                            {
                                continue;
                            }

                            ret_handshake = lockdown.lockdownd_client_new_with_handshake(iDeviceHandle,
                                out lockdownClientHandle, "waua");
                            if (ret_handshake != 0)
                            {
                                continue;
                            }

                            var ret_get_value = lockdown.lockdownd_get_value(lockdownClientHandle, null,
                                "ProductVersion", out var node);
                            if (ret_get_devname != 0)
                            {
                                continue;
                            }

                            LibiMobileDevice.Instance.Plist.plist_get_string_val(node, out var version);

                            ret_get_value = lockdown.lockdownd_get_value(lockdownClientHandle, null,
                                "BuildVersion", out node);
                            if (ret_get_devname != 0)
                            {
                                continue;
                            }

                            LibiMobileDevice.Instance.Plist.plist_get_string_val(node, out var bldVersion);

                            iDeviceHandle.Dispose();
                            lockdownClientHandle.Dispose();
                            var device = new DeviceModel
                            {
                                UDID = udid,
                                Name = deviceName,
                                Version = version,
                                BuildVersion = bldVersion,
                                ShortVersion = string.Join(".", version.Split('.').Take(2)),
                                FullVersion = string.Join(".", version.Split('.').Take(2)) + "(" + bldVersion + ")",
                                isDevMode = device_utils.is_device_on_dev_mode(udid)
                            };

                            device_add_remove(device, dev_op.add_device);
                        }
                    }
                    else
                    {
                        device_add_remove(new DeviceModel(), dev_op.clear_all);
                    }
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

                    wnd_instance.Dispatcher.Invoke((Action)(() =>
                    {
                        if (Devices.Count <= 1)
                        {
                            wnd_instance.connected_dev.Text = "Device "
                            + device.Name + "(" + device.Version +
                            ") is connected!";
                        }
                        else
                        {
                            wnd_instance.connected_dev.Text += "Device "
                            + device.Name + "(" + device.Version +
                            ") is connected!";
                        }
                    }));

                    Devices.Add(device);
                    break;

                case dev_op.remove_device:
                    wnd_instance.Dispatcher.Invoke((Action)(() =>
                    {
                        wnd_instance.connected_dev.Text += "Device "
                        + device.Name + "(" + device.Version +
                        ") is disconnected!";
                    }));

                    Devices.Remove(device);
                    break;

                case dev_op.clear_all:
                    wnd_instance.Dispatcher.Invoke((Action)(() =>
                    {
                        wnd_instance.connected_dev.Text = "No device is connected!";
                    }));

                    Devices.Clear();
                    break;

                default: break;
            }

            wnd_instance.Dispatcher.Invoke((Action)(() =>
            {
                wnd_instance.device_prov.IsEnabled = (Devices.Count >= 1);
            }));

        }

        public void UpdateLocation(Location location)
        {
            // no device is connected.
            if (Devices.Count == 0)
            {
                return;
            }

            iDevice.idevice_set_debug_level(1);

            var Longitude = location.Longitude.ToString();
            var Latitude = location.Latitude.ToString();

            var size = BitConverter.GetBytes(0u);
            Array.Reverse(size);

            for (int i = 0; i < Devices.Count(); i++)
            {
                DeviceModel itm = Devices[i];
                var num = 0u;
                iDevice.idevice_new(out var device, itm.UDID);

                var ret_handshake = lockdown.lockdownd_client_new_with_handshake(device, out var client,
                    "devicelocation");
                if (ret_handshake != 0)
                {
                    continue;
                }

                if (!itm.isDevMode)
                {
                    continue;
                }

                var ret_start_service = lockdown.lockdownd_start_service(client,
                    "com.apple.dt.simulatelocation", out var service2);
                if (ret_start_service != 0)
                {
                    continue;
                }

                var se = service.service_client_new(device, service2, out var client2);

                se = service.service_send(client2, size, 4u, ref num);

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

                device.Dispose();
                client.Dispose();
                client2.Dispose();

                service2 = null;
            }
        }

        public void ClearLocation()
        {
            if (Devices.Count == 0)
            {
                return;
            }

            iDevice.idevice_set_debug_level(1);

            foreach (var itm in Devices)
            {
                var num = 0u;
                iDevice.idevice_new(out var device, itm.UDID);
                var lockdowndError = lockdown.lockdownd_client_new_with_handshake(device,
                    out LockdownClientHandle client, "com.alpha.jailout");
                if (lockdowndError != 0)
                {
                    continue;
                }

                lockdowndError = lockdown.lockdownd_start_service(client, "" +
                    "com.apple.dt.simulatelocation", out var service2);
                if (lockdowndError != 0)
                {
                    continue;
                }

                var se = service.service_client_new(device, service2, out var client2);
                if (se != 0)
                {
                    continue;
                }

                se = service.service_send(client2, new byte[4] { 0, 0, 0, 0 }, 4, ref num);
                if (se != 0)
                {
                    continue;
                }

                se = service.service_send(client2, new byte[4] { 0, 0, 0, 1 }, 4, ref num);
                if (se != 0)
                {
                    continue;
                }

                device.Dispose();
                client.Dispose();
            };
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

