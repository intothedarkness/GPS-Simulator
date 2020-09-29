//
//  Created by Richard Zhang (Richard.Rupo.Zhang@gmail.com) on 3/2020
//  Copyright © 2020 Richard Zhang. All rights reserved.
//

// Bing Map WPF control
using Microsoft.Maps.MapControl.WPF;

// GPX
using SharpGpx;

using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Xml.Linq;

namespace GPS_Simulator
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// load GPX
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void option_Button_Click(object sender, RoutedEventArgs e)
        {
            OptionDlg();
        }

        /// <summary>
        ///  load GPX track files.
        /// </summary>
        public void OptionDlg()
        {
            OpenFileDialog gpx_open_dlg = new OpenFileDialog
            {
                InitialDirectory = @"D:\",
                Title = "Browse GPX Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "GPX",
                Filter = "GPX files (*.gpx)|*.gpx",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            var result = gpx_open_dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (g_gpx_file_name != gpx_open_dlg.FileName
                    && gpx_open_dlg.FileName != null)
                {
                    // clear the previous route
                    myMap.Children.Remove(g_polyline);

                    g_gpx_file_name = gpx_open_dlg.FileName;

                    // draw route
                    draw_gpx_route();
                }
            }
        }

        private static void read_gpx_coords(string gpx_file_name, ref LocationCollection lc)
        {
            GpxClass gpx = GpxClass.FromFile(gpx_file_name);

            foreach (var wpt in gpx.wpt)
            {
                lc.Add(new Location(Convert.ToDouble(wpt.lat),
                    Convert.ToDouble(wpt.lon),
                    Convert.ToDouble(wpt.ele)));
            }

            foreach (var trk in gpx.trk)
            {
                foreach (var trk_seg in trk.trkseg)
                {
                    foreach (var trkpt in trk_seg.trkpt)
                    {
                        lc.Add(new Location(Convert.ToDouble(trkpt.lat),
                                Convert.ToDouble(trkpt.lon),
                                Convert.ToDouble(trkpt.ele)));
                    }
                }
            }
        }
        /// <summary>
        /// Draw GPX tracks on the map.
        /// </summary>
        public void draw_gpx_route()
        {
            if (g_gpx_file_name == null
                || g_gpx_file_name.Length == 0)
            {
                return;
            }

            LocationCollection lc = new LocationCollection();
            read_gpx_coords(g_gpx_file_name, ref lc);

            // draw the tack on map
            MapPolyline polyline = new MapPolyline();
            polyline.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.
                Colors.Blue);

            polyline.StrokeThickness = 3;
            polyline.Opacity = 0.7;

            polyline.Locations = lc;

            myMap.Children.Add(polyline);

            g_polyline = polyline;

            myMap.Center = lc[0];

            // set the walking to the beginning of new route
            if (timer_callback != null)
            {
                timer_callback.set_route(g_polyline);
                cur_walking_state = e_walking_state.walking_stopped;
                walking.Content = "Start";
            }
        }

        /// <summary>
        /// set speeds.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void walking_speed_click(object sender, RoutedEventArgs e)
        {
            timer_callback.walking_speed = c_fast_walking_speed;
        }

        private void driving_speed_click(object sender, RoutedEventArgs e)
        {
            timer_callback.walking_speed = c_fast_walking_speed * 12;
        }

        private void running_speed_click(object sender, RoutedEventArgs e)
        {
            timer_callback.walking_speed = c_fast_walking_speed * 3;
        }

        private void stop_at_end_click(object sender, RoutedEventArgs e)
        {
            cur_routing_mode = e_routing_mode.stop_at_end;
        }

        private void loop_to_start_click(object sender, RoutedEventArgs e)
        {
            cur_routing_mode = e_routing_mode.loop_to_start;
        }

        private void loop_reverse_click(object sender, RoutedEventArgs e)
        {
            cur_routing_mode = e_routing_mode.reverse_walk;
        }

        /// <summary>
        ///  start to walk and auto repeat.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void walk_Button_Click(object sender, RoutedEventArgs e)
        {
            if (g_gpx_file_name == null)
            {
                System.Windows.Forms.MessageBox.Show("Please load a GPX file and then walk.");
                return;
            }

            // initialize the timer call back
            if (timer_callback == null)
            {
                timer_callback = new walking_timer_callback(g_polyline, myMap, this);
            }

            if (timer_callback.m_polyline == null)
            {
                timer_callback.set_route(g_polyline);
            }

            switch (cur_walking_state)
            {
                case e_walking_state.walking_stopped:
                    // stopped -- > active
                    walking.Content = "Pause"; // indicate use can pause in active.
                    walking_timer.Start();
                    option.IsEnabled = false;
                    cur_walking_state = e_walking_state.walking_active;
                    break;

                case e_walking_state.walking_paused:
                    // paused -- > active
                    walking.Content = "Pause"; // indicate use can pause in active.
                    walking_timer.Start();
                    option.IsEnabled = false;
                    cur_walking_state = e_walking_state.walking_active;
                    break;

                case e_walking_state.walking_active:
                    // active --> paused
                    walking.Content = "Resume"; // indicate use can resume in paused.
                    walking_timer.Stop();
                    option.IsEnabled = true;
                    cur_walking_state = e_walking_state.walking_paused;
                    break;

                default: break;
            }
        }

        public void switch_walking_state()
        {
            switch (cur_walking_state)
            {
                case e_walking_state.walking_stopped:
                    // do nothing. it is already stopped.
                    break;

                case e_walking_state.walking_active:
                case e_walking_state.walking_paused:
                    // reset the current position to the beginning.
                    walking.Content = "Start"; // indicate use can start in stopped.
                    walking_timer.Stop();
                    option.IsEnabled = true; // user can load new route.

                    // reset to the beginning of the route.
                    timer_callback.reset();
                    cur_walking_state = e_walking_state.walking_stopped;
                    break;

                default: break;
            }
        }

        private void stop_Button_Click(object sender, RoutedEventArgs e)
        {
            switch_walking_state();
        }
    }
}