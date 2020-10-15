//
//  Created by Richard Zhang (Richard.Rupo.Zhang@gmail.com) on 3/2020
//  Copyright © 2020 Richard Zhang. All rights reserved.
//

// Bing Map WPF control
using Microsoft.Maps.MapControl.WPF;

using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Xml;

namespace GPS_Simulator
{
    public partial class MainWindow : Window
    {
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
                tele.Altitude = Convert.ToDouble(alt.Text);
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

        private void teleport_click(object sender, MouseButtonEventArgs e)
        {
            // Disables double-click teleport when it is in walking mode.
            if (cur_walking_state == e_walking_state.walking_active)
            {
                System.Windows.Forms.MessageBox.Show("Quit from walking mode first.");
                return;
            }

            // Disables the default mouse double-click action.
            e.Handled = true;

            
            // Convert the mouse coordinates to a location on the map
            Location pinLocation = GetMapLocation(e);

            // The pushpin to add to the map.
            if (teleport_pin != null)
            {
                myMap.Children.Remove(teleport_pin);
            }
            else
            {
                teleport_pin = new Pushpin();
            }

            string elevationUrl = spell_elevation_query_url(pinLocation);
            List<double> elevations = get_elevations(elevationUrl);
            if (elevations.Count > 0)
            {
                pinLocation.Altitude = elevations[0];
            }

            teleport_pin.Location = pinLocation;

            // Adds the pushpin to the map.
            myMap.Children.Add(teleport_pin);

            // update the coords
            lat.Text = pinLocation.Latitude.ToString();
            lon.Text = pinLocation.Longitude.ToString();
            alt.Text = pinLocation.Altitude.ToString();

            location_service.GetInstance(this).UpdateLocation(pinLocation);
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
                default: break;
            }
        }



        private XmlDocument GetXmlResponse(string requestUrl)
        {
            XmlDocument xmlDoc = new XmlDocument();

            HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    xmlDoc.Load(response.GetResponseStream());
                }
            }

            return xmlDoc;
        }

        // Format the URI from a list of locations.
        protected string spell_elevation_query_url(List<list_item> locList)
        {
            // The base URI string. Fill in: 
            // {0}: The lat/lon list, comma separated. 
            // {1}: The key. 
            const string BASE_URI_STRING =
              "http://dev.virtualearth.net/REST/v1/Elevation/List?points={0}&key={1}&o=xml";

            string retVal = string.Empty;
            string locString = string.Empty;
            for (int ndx = 0; ndx < locList.Count; ++ndx)
            {
                if (ndx != 0)
                {
                    locString += ",";
                }
                locString += locList[ndx].loc.Latitude.ToString() + "," + locList[ndx].loc.Longitude.ToString();
            }
            retVal = string.Format(BASE_URI_STRING, locString, BingMapKey);
            return retVal;
        }

        // spell the url for single point.
        protected string spell_elevation_query_url(Location loc)
        {
            // The base URI string. Fill in: 
            // {0}: The lat/lon list, comma separated. 
            // {1}: The key. 
            const string BASE_URI_STRING =
              "http://dev.virtualearth.net/REST/v1/Elevation/List?points={0}&key={1}&o=xml";

            string locString = loc.Latitude.ToString() + "," + loc.Longitude.ToString();
            return string.Format(BASE_URI_STRING, locString, BingMapKey);
        }

        protected List<double> get_elevations(string url)
        {
            List<double> ret = new List<double>();
            XmlDocument res = GetXmlResponse(url);
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(res.NameTable);
            nsmgr.AddNamespace("rest", "http://schemas.microsoft.com/search/local/ws/rest/v1");

            XmlNode elevationSets = res.SelectSingleNode("//rest:Elevations", nsmgr);
            foreach (XmlNode node in elevationSets.ChildNodes)
            {
                ret.Add(Convert.ToDouble(node.InnerText));
            }

            return ret;
        }

        private void search_Button_Click(object sender, RoutedEventArgs e)
        {
            search_result_list.Items.Clear();
            g_query_result.Clear();

            if (search_box.Text.Length <= 0)
            {
                return;
            }

            string requestUrl = @"http://dev.virtualearth.net/REST/v1/Locations/" + search_box.Text.Trim() + "?o=xml&key=" + BingMapKey;

            // Make the request and get the response
            XmlDocument geocodeResponse = GetXmlResponse(requestUrl);

            // Create namespace manager
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(geocodeResponse.NameTable);
            nsmgr.AddNamespace("rest", "http://schemas.microsoft.com/search/local/ws/rest/v1");

            // Get all geocode locations in the response 
            XmlNodeList locationElements = geocodeResponse.SelectNodes("//rest:Location", nsmgr);

            if (locationElements.Count <= 0)
            {
                return;
            }

            for (int i = 0; i < locationElements.Count; i++)
            {
                Location loc = new Location();
                loc.Latitude = Convert.ToDouble(locationElements[i].SelectSingleNode(".//rest:Latitude", nsmgr).InnerText);
                loc.Longitude = Convert.ToDouble(locationElements[i].SelectSingleNode(".//rest:Longitude", nsmgr).InnerText);

                list_item it = new list_item();
                it.loc = loc;
                it.Name = locationElements[i].SelectSingleNode(".//rest:Name", nsmgr).InnerText;

                g_query_result.Add(it);
                search_result_list.Items.Add(it.Name);
            }

            // get the elevations for these addresses.
            string elevationUrl = spell_elevation_query_url(g_query_result);
            List<double> alt_list = get_elevations(elevationUrl);

            for (int i = 0; i < g_query_result.Count; i++)
            {
                g_query_result[i].loc.Altitude = alt_list[i];
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
                alt.Text = it.loc.Altitude.ToString();
            }
        }

        private void search_box_key_down_handler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                search_Button_Click(0, new System.Windows.RoutedEventArgs());
            }
        }
    }
}