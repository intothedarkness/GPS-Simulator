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
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;

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

        public e_walking_state cur_walking_state = e_walking_state.walking_stopped;

        public static string g_gpx_file_name = null;
        public MapPolyline g_polyline = null;
        private static DispatcherTimer walking_timer = null;
        private static walking_timer_callback timer_callback = null;
        location_service loc_service = null;
        public Pushpin teleport_pin = null;

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
            XDocument gpx_file = XDocument.Load(gpx_file_name);
            XNamespace gpx = XNamespace.Get("http://www.topografix.com/GPX/1/1");

            var waypoints = from waypoint in gpx_file.Descendants(gpx + "wpt")
                            select new
                            {
                                Latitude = waypoint.Attribute("lat").Value,
                                Longitude = waypoint.Attribute("lon").Value,
                                Elevation = waypoint.Element(gpx + "ele") != null ? waypoint.Element(gpx + "ele").Value : null
                            };

            foreach (var wpt in waypoints)
            {
                lc.Add(new Location(Convert.ToDouble(wpt.Latitude),
                    Convert.ToDouble(wpt.Longitude),
                    Convert.ToDouble(wpt.Elevation)));
            }

            var tracks = from track in gpx_file.Descendants(gpx + "trk")
                         select new
                         {
                             Name = track.Element(gpx + "name") != null ? track.Element(gpx + "name").Value : null,
                             Segs = (
                             from trackpoint in track.Descendants(gpx + "trkpt")
                             select new
                             {
                                 Latitude = trackpoint.Attribute("lat").Value,
                                 Longitude = trackpoint.Attribute("lon").Value,
                                 Elevation = trackpoint.Element(gpx + "ele") != null ? trackpoint.Element(gpx + "ele").Value : null
                             }
                             )
                         };

            foreach (var trk in tracks)
            {
                // Populate track data.
                foreach (var trkSeg in trk.Segs)
                {
                    lc.Add(new Location(Convert.ToDouble(trkSeg.Latitude),
                        Convert.ToDouble(trkSeg.Longitude),
                        Convert.ToDouble(trkSeg.Elevation)));
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
        ///  teleport to a location.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tele_Button_Click(object sender, RoutedEventArgs e)
        {
            if (cur_walking_state != e_walking_state.walking_stopped)
            {
                System.Windows.Forms.MessageBox.Show("Quit from walking mode first.");
                return;
            }

            Location tele = new Location();

            try
            {
                tele.Latitude = Convert.ToDouble(lat.Text);
                tele.Longitude = Convert.ToDouble(lon.Text);
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("the GPS coordination is invalid!");
                return;
            }

            // The pushpin to add to the map.
            if (teleport_pin != null)
            {
                myMap.Children.Remove(teleport_pin);
            }
            else
            {
                teleport_pin = new Pushpin();
            }

            teleport_pin.Location = tele;

            // Adds the pushpin to the map.
            myMap.Center = tele;
            myMap.Children.Add(teleport_pin);

            location_service.GetInstance(this).UpdateLocation(tele);
        }

        /// <summary>
        /// double click and teleport.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Map_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Disables double-click teleport when it is in walking mode.
            if (cur_walking_state == e_walking_state.walking_active)
            {
                System.Windows.Forms.MessageBox.Show("Quit from walking mode first.");
                return;
            }

            // Disables the default mouse double-click action.
            e.Handled = true;

            // Determine the location to place the pushpin at on the map.

            // Get the mouse click coordinates
            Point mousePosition = e.GetPosition(this);

            // WARNING:
            // It seems to be a bug of Bing Map WPF control, that when the control is 
            // not in full screen mode, the coords calculation got some offsets. 
            // make a dirty adjustment here.
            mousePosition.Offset(-Width * 3 / 16, 0);

            // Convert the mouse coordinates to a location on the map
            Location pinLocation = myMap.ViewportPointToLocation(mousePosition);

            // The pushpin to add to the map.
            if (teleport_pin != null)
            {
                myMap.Children.Remove(teleport_pin);
            }
            else
            {
                teleport_pin = new Pushpin();
            }

            teleport_pin.Location = pinLocation;

            // Adds the pushpin to the map.
            myMap.Children.Add(teleport_pin);

            // update the coords
            lat.Text = pinLocation.Latitude.ToString();
            lon.Text = pinLocation.Longitude.ToString();

            location_service.GetInstance(this).UpdateLocation(pinLocation);
        }

        private void device_Button_Click(object sender, RoutedEventArgs e)
        {
            dev_prov dlg = new dev_prov(this, location_service.GetInstance(this).Devices);
            dlg.ShowDialog();
        }

        private void stop_Button_Click(object sender, RoutedEventArgs e)
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

        private XmlDocument GetXmlResponse(string requestUrl)
        {
            System.Diagnostics.Trace.WriteLine("Request URL (XML): " + requestUrl);
            HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception(String.Format("Server error (HTTP {0}: {1}).",
                    response.StatusCode,
                    response.StatusDescription));
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(response.GetResponseStream());
                return xmlDoc;
            }
        }

        private double ConvertToDouble(string s)
        {
            char systemSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator[0];
            double result = 0;
            try
            {
                if (s != null)
                    if (!s.Contains(","))
                        result = double.Parse(s, CultureInfo.InvariantCulture);
                    else
                        result = Convert.ToDouble(s.Replace(".", systemSeparator.ToString()).Replace(",", systemSeparator.ToString()));
            }
            catch (Exception)
            {
                try
                {
                    result = Convert.ToDouble(s);
                }
                catch
                {
                    try
                    {
                        result = Convert.ToDouble(s.Replace(",", ";").Replace(".", ",").Replace(";", "."));
                    }
                    catch
                    {
                        throw new Exception("Wrong string-to-double format");
                    }
                }
            }
            return result;
        }

        private void search_Button_Click(object sender, RoutedEventArgs e)
        {
            search_result_list.Items.Clear();
            g_query_result.Clear();

            if (search_box.Text.Length <= 0)
            {
                return;
            }

            string BingMapKey = @"MRoghxvRwiH04GVvGpg4~uaP_it5CCQ6ckz-j9tA_iQ~AoPUZFQPIn9s1qjKPLgkvgeGPZPKznUlqM_e0fPu8NCXTi_ZSZTDud4_j0F1SkKU";
            string requestUrl = @"http://dev.virtualearth.net/REST/v1/Locations/" + search_box.Text + "?o=xml&key=" + BingMapKey;

            // Make the request and get the response
            XmlDocument geocodeResponse = GetXmlResponse(requestUrl);

            // Create namespace manager
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(geocodeResponse.NameTable);
            nsmgr.AddNamespace("rest", "http://schemas.microsoft.com/search/local/ws/rest/v1");

            // Get all geocode locations in the response 
            XmlNodeList locationElements = geocodeResponse.SelectNodes("//rest:Location", nsmgr);

            foreach (XmlNode locnode in locationElements)
            {
                Location loc = new Location();
                loc.Latitude = ConvertToDouble(locnode.SelectSingleNode(".//rest:Latitude", nsmgr).InnerText);
                loc.Longitude = ConvertToDouble(locnode.SelectSingleNode(".//rest:Longitude", nsmgr).InnerText);

                list_item it = new list_item();
                it.loc = loc;
                it.Name = locnode.SelectSingleNode(".//rest:Name", nsmgr).InnerText;

                g_query_result.Add(it);
                search_result_list.Items.Add(it.Name);
            }
        }

        private void search_result_list_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (g_query_result.Count <= 0 || search_result_list.SelectedIndex < 0)
            {
                return;
            }
            else
            {
                list_item it = g_query_result[search_result_list.SelectedIndex];
                lat.Text = it.loc.Latitude.ToString();
                lon.Text = it.loc.Longitude.ToString();
            }
        }
    }
}
