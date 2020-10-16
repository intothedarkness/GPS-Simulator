
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
using System.Linq;
using System.Windows;
using Forms = System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Media;
using System.Net.NetworkInformation;

namespace GPS_Simulator
{
    public partial class MainWindow : Window
    {

        List<Pushpin> pins = new List<Pushpin>();

        private void gpx_create_Click(object sender, RoutedEventArgs e)
        {
            switch (cur_click_mode)
            {
                case e_click_mode.create_gpx: // creating mode -->teleport mode
                    cur_click_mode = e_click_mode.teleport;
                    gpx_create_button.Content = "Create GPX";
                    way_points.Text = "";
                    gpx_locations.Clear();
                    pins.Clear();
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
            Forms.SaveFileDialog saveFileDialog1 = new Forms.SaveFileDialog();
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

        private Pushpin CreatePushpinAndAddToMap(Location pinLocation)
        {
            Pushpin waypoint_pin = new Pushpin();
            waypoint_pin.Location = pinLocation;
            waypoint_pin.MouseDown += Pin_MouseDown;
            waypoint_pin.MouseUp += Pin_MouseUp;

            string elevationUrl = spell_elevation_query_url(pinLocation);
            List<double> elevations = get_elevations(elevationUrl);
            if (elevations.Count > 0)
            {
                pinLocation.Altitude = elevations[0];
            }

            // Adds the pushpin to the map.
            myMap.Children.Add(waypoint_pin);
            return waypoint_pin;
        }
        

        private void gpx_add_waypoint(MouseButtonEventArgs e)
        {
            if (cur_click_mode != e_click_mode.create_gpx)
                return;

            e.Handled = true;

            if (teleport_pin != null)
            {
                myMap.Children.Remove(teleport_pin);
            }

            // Convert the mouse coordinates to a location on the map
            Location pinLocation = GetMapLocation(e);

            Pushpin waypoint_pin = CreatePushpinAndAddToMap(pinLocation);

            pins.Add(waypoint_pin);
            gpx_locations.Add(pinLocation);

            if (gpx_locations.Count > 0)
                gpx_save_button.IsEnabled = true;

            gpx_update_from_gpx_locations();
        }

        private void gpx_update_from_gpx_locations()
        {
            gpx_update_polyline();
            gpx_update_waypoints_text();
            gpx_update_pins();
        }

        private void gpx_update_polyline()
        {
            // draw the tack on map
            MapPolyline polyline = new MapPolyline();
            polyline.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.
                Colors.Blue);

            polyline.StrokeThickness = 3;
            polyline.Opacity = 0.7;

            polyline.Locations = gpx_locations;
            myMap.Children.Add(polyline);
        }

        private void gpx_update_waypoints_text()
        {
            way_points.Text = string.Join("\n",
                gpx_locations.Select(loc => $"WayPoint({loc.Longitude:F4}, {loc.Latitude:F4}, {loc.Altitude})")
            );
        }

        Color startColor = Colors.ForestGreen;
        Color endColor = Colors.DarkRed;
        Color routeColor = Colors.DarkBlue;
        private void gpx_update_pins()
        {
            // color the pins so we know where the start and ending pin is
            // first pin is green and last pin is red
            foreach(var pin in pins)
            {
                pin.Background = new SolidColorBrush(routeColor);
            }
            pins.Last().Background = new SolidColorBrush(endColor);
            pins.First().Background = new SolidColorBrush(startColor);

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

        /// <summary>
        /// double click and teleport.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Map_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            switch (cur_click_mode)
            {
                case e_click_mode.teleport:
                    teleport_click(sender, e);
                    break;
                case e_click_mode.create_gpx:
                    gpx_add_waypoint(e);
                    break;
                default: break;
            }
        }

        private Pushpin selectedPushpin;
        private bool inPushpinDrag = false;
        private Location oldPinLocation;
        private Vector mouseToMarker;

        private void Pin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            // we only care about this event on a pushpin
            var pushpin = sender as Pushpin;
            if (pushpin == null) return;

            if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
            {
                // if the left button was pushed down on a bin, then begin "pushpin dragging" mode
                selectedPushpin = pushpin;
                inPushpinDrag = true;
                oldPinLocation = selectedPushpin.Location;
                mouseToMarker = Point.Subtract(
                  myMap.LocationToViewportPoint(selectedPushpin.Location),
                  e.GetPosition(myMap));
            }
        }

        private void Map_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed &&
                inPushpinDrag && selectedPushpin != null)
            {
                // if we're in dragging mode and the button is down, then move the location of the pin
                // we won't update the line until the button is released
                selectedPushpin.Location = myMap.ViewportPointToLocation(
                    Point.Add(e.GetPosition(myMap), mouseToMarker));
                e.Handled = true;
            }
        }

        private void Pin_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && inPushpinDrag)
            {
                // update the location associated with the old pin location in the location list
                int index = gpx_locations.IndexOf(oldPinLocation);
                gpx_locations[index] = selectedPushpin.Location;

                //update the line on the map and the text of the waypoints
                gpx_update_from_gpx_locations();
                // we're no longer in "pushpin dragging" mode
                inPushpinDrag = false;
            } else if (e.ChangedButton == MouseButton.Right)
            {
                // on right clicking a pin, show the context menu
                ContextMenu cm = FindResource("cmPushpin") as ContextMenu;
                cm.PlacementTarget = sender as Pushpin;
                // save a reference to the pin as the DataContext so we can grab it later
                cm.DataContext = sender as Pushpin;
                // show the menu
                cm.IsOpen = true;
                e.Handled = true;
            }
        }

        private void AddLocationBetweenLocationAndNext(int locIndex)
        {
            Debug.Assert(locIndex < gpx_locations.Count);

            var loc1 = gpx_locations[locIndex];
            var loc2 = gpx_locations[locIndex + 1];
            
            // calculate the midpoint between this location and the next one
            var midLoc = new Location((loc1.Latitude + loc2.Latitude) / 2,
                (loc1.Longitude + loc2.Longitude) / 2,
                (loc1.Altitude + loc2.Altitude) / 2);

            // insert it after the location in the list of locations
            gpx_locations.Insert(locIndex + 1, midLoc);

            // create the pushpin for this and add it as well
            Pushpin pin = CreatePushpinAndAddToMap(midLoc);
            pins.Insert(locIndex + 1, pin);

        }

        public void AddPinBeforeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"Add pin before");
            var control = sender as Control;
            var pin = control.DataContext as Pushpin;

            int locIndex = gpx_locations.IndexOf(pin.Location);

            // can't add a pin before the first pin
            if (locIndex > 0)
            {
                AddLocationBetweenLocationAndNext(locIndex - 1);
                gpx_update_from_gpx_locations();
            }
        }

        public void AddPinAfterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"Add pin after");
            var control = sender as Control;
            var pin = control.DataContext as Pushpin;

            int locIndex = gpx_locations.IndexOf(pin.Location);

            // can't add a pin after the last one
            if (locIndex < gpx_locations.Count - 1)
            {
                AddLocationBetweenLocationAndNext(locIndex);
                gpx_update_from_gpx_locations();
            }
        }

        public void RemovePinMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"Removing pin");
            var control = sender as Control;
            var pin = control.DataContext as Pushpin;

            //remove the pushpin location from the route and redraw the route.
            int locIndex = gpx_locations.IndexOf(pin.Location);
            if (locIndex < 0)
            {
                throw new Exception("can't find the location index for the removed pin location");
            }
            
            // remove the pin from the map and the list of pins
            myMap.Children.Remove(pin);
            pins.Remove(pin);

            // remove the location from the location list
            gpx_locations.RemoveAt(locIndex);

            // update the text and route line
            gpx_update_from_gpx_locations();
            
        }


    }
}