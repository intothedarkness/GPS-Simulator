//
//  Created by Richard Zhang (Richard.Rupo.Zhang@gmail.com) on 3/2020
//  Copyright © 2020 Richard Zhang. All rights reserved.
//
// libimobiledevice-net references
using iMobileDevice;

// Bing Map WPF control
using Microsoft.Maps.MapControl.WPF;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;

// GPX

namespace GPS_Simulator
{
    public class list_item
    {
        public Location loc { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // fast walking speed is at 8.8km/h == 2.47 m/s
        const double c_fast_walking_speed = 2.47f;

        // Current query result
        public List<list_item> g_query_result = new List<list_item>();

        public enum e_walking_state
        {
            walking_active = 1,
            walking_paused = 2,
            walking_stopped = 3
        }

        public enum e_click_mode
        {
            create_gpx = 1,
            teleport = 2
        }

        public enum e_routing_mode
        {
            stop_at_end = 1,
            loop_to_start = 2,
            reverse_walk = 3
        }

        public e_click_mode cur_click_mode = e_click_mode.teleport;
        public e_walking_state cur_walking_state = e_walking_state.walking_stopped;
        public e_routing_mode cur_routing_mode = e_routing_mode.reverse_walk;

        public static string g_gpx_file_name = null;
        public MapPolyline g_polyline = null;
        private static DispatcherTimer walking_timer = null;
        private static walking_timer_callback timer_callback = null;
        location_service loc_service = null;
        public Pushpin teleport_pin = null;

        public bool is_spoofing_on = false;

        LocationCollection gpx_locations = new LocationCollection();

        public string BingMapKey = @"MRoghxvRwiH04GVvGpg4~uaP_it5CCQ6ckz-j9tA_iQ~AoPUZFQPIn9s1qjKPLgkvgeGPZPKznUlqM_e0fPu8NCXTi_ZSZTDud4_j0F1SkKU";

        /// <summary>
        /// main window initialization
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // default is walking
            walking_speed.IsChecked = true;
            running_speed.IsChecked = false;
            driving_speed.IsChecked = false;

            // default is reverse walking
            stop_at_end.IsChecked = false;
            loop_to_start.IsChecked = false;
            loop_reverse.IsChecked = true;

            gpx_save_button.IsEnabled = false;

            stop_spoofing_button.IsEnabled = false;

            // load native libraries for iDevice
            NativeLibraries.Load();

            // init walking timer.
            walking_timer = new DispatcherTimer();
            walking_timer.Interval = TimeSpan.FromMilliseconds(500); // 0.5 sec
            timer_callback = new walking_timer_callback(g_polyline, myMap, this);
            timer_callback.walking_speed = c_fast_walking_speed;
            walking_timer.Tick += timer_callback.one_step;
            walking_timer.IsEnabled = true;
            walking_timer.Stop();

            loc_service = location_service.GetInstance(this);
            loc_service.ListeningDevice();

            if (loc_service.Devices.Count < 1)
            {
                device_prov.IsEnabled = false;
            }

            string ddi_path = AppDomain.CurrentDomain.BaseDirectory + "DDILocalRepo\\";
            if (!System.IO.Directory.Exists(ddi_path))
                System.IO.Directory.CreateDirectory(ddi_path);

            // set map center.
            Location map_center = new Location();
            map_center.Latitude = Properties.Settings.Default.home_lat;
            map_center.Longitude = Properties.Settings.Default.home_lon;

            myMap.Center = map_center;
        }
    }
}
