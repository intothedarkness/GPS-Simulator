
//
//  Created by Richard Zhang (Richard.Rupo.Zhang@gmail.com) on 3/2020
//  Copyright © 2020 Richard Zhang. All rights reserved.
//

// Bing Map WPF control
using Microsoft.Maps.MapControl.WPF;

// GPX
using SharpGpx;
using SharpGpx.GPX1_1;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace GPS_Simulator
{
    public partial class MainWindow : Window
    {
        private void gpx_create_Click(object sender, RoutedEventArgs e)
        {
            switch (cur_click_mode)
            {
                case e_click_mode.create_gpx: // creating mode -->teleport mode
                    cur_click_mode = e_click_mode.teleport;
                    gpx_create_button.Content = "Create GPX";
                    way_points.Text = "";
                    gpx_locations.Clear();
                    myMap.Children.Clear();
                    gpx_save_button.IsEnabled = false;
                    break;

                case e_click_mode.teleport:
                    System.Windows.Forms.MessageBox.Show("entering GPX creation mode, single left click to set way point, right click to reset the waypoints, click the \"Save GPX\" button to save it to a GPX file.");
                    cur_click_mode = e_click_mode.create_gpx;
                    gpx_create_button.Content = "Back to Teleport Mode";
                    break;
            }
        }

        private void gpx_save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "GPX file|*.gpx";
            saveFileDialog1.Title = "Save To GPX File";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                GpxClass track = new GpxClass();

                wptTypeCollection trkpt = new wptTypeCollection();
                foreach (Location lc in gpx_locations)
                {
                    wptType wpt = new wptType();
                    wpt.lat = (decimal)lc.Latitude;
                    wpt.lon = (decimal)lc.Longitude;
                    wpt.ele = (decimal)lc.Altitude;
                    wpt.eleSpecified = true;
                    trkpt.Add(wpt);
                }

                trksegType trk_seg = new trksegType();
                trk_seg.trkpt = trkpt;

                trksegTypeCollection trk_seg_cl = new trksegTypeCollection();
                trk_seg_cl.Addtrkseg(trk_seg);

                trkType trk = new trkType();
                trk.name = DateTime.Now.ToString("MM/dd/yyyy h:mm tt");
                trk.trkseg = trk_seg_cl;

                track.trk = new trkTypeCollection();
                track.trk.Add(trk);
                track.ToFile(saveFileDialog1.FileName);

                System.Windows.Forms.MessageBox.Show("GPX file is saved!");

                // back to the teleport mode.
                cur_click_mode = e_click_mode.teleport;
                gpx_create_button.Content = "Create GPX";
                way_points.Text = "";
                gpx_locations.Clear();
                myMap.Children.Clear();
                gpx_save_button.IsEnabled = false;
            }
        }

        private void Map_MouseSingleLeftClick(object sender, MouseButtonEventArgs e)
        {
            if (cur_click_mode != e_click_mode.create_gpx)
                return;

            e.Handled = true;

            Point mousePosition = e.GetPosition(this);
            mousePosition.Offset(-Width * 3 / 16, 0);

            if (teleport_pin != null)
            {
                myMap.Children.Remove(teleport_pin);
            }

            Pushpin waypoint_pin = new Pushpin();

            // Convert the mouse coordinates to a location on the map
            Location pinLocation = myMap.ViewportPointToLocation(mousePosition);

            waypoint_pin.Location = pinLocation;
            string elevationUrl = spell_elevation_query_url(pinLocation);
            List<double> elevations = get_elevations(elevationUrl);
            if (elevations.Count > 0)
            {
                pinLocation.Altitude = elevations[0];
            }

            // Adds the pushpin to the map.
            myMap.Children.Add(waypoint_pin);
            gpx_locations.Add(pinLocation);

            if (gpx_locations.Count > 0)
                gpx_save_button.IsEnabled = true;

            // draw the tack on map
            MapPolyline polyline = new MapPolyline();
            polyline.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.
                Colors.Blue);

            polyline.StrokeThickness = 3;
            polyline.Opacity = 0.7;

            polyline.Locations = gpx_locations;
            myMap.Children.Add(polyline);

            way_points.Text += pinLocation.Longitude.ToString() + "," + pinLocation.Latitude.ToString() + "," + pinLocation.Altitude.ToString() + "\n";

        }

        private void Map_MouseSingleRightClick(object sender, MouseButtonEventArgs e)
        {
            switch (cur_click_mode)
            {
                case e_click_mode.teleport:
                    break;

                case e_click_mode.create_gpx:
                    // clear the gpx buffer.
                    way_points.Text = "";
                    gpx_locations.Clear();
                    myMap.Children.Clear();
                    gpx_save_button.IsEnabled = false;
                    break;
            }
        }
    }
}