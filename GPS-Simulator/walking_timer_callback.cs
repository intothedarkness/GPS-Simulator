//
//  Created by Richard Zhang (Richard.Rupo.Zhang@gmail.com) on 3/2020
//  Copyright © 2020 Richard Zhang. All rights reserved.
//
// Bing MAP WPF reference
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Linq;

namespace GPS_Simulator
{
    class walking_timer_callback
    {
        public Location m_cur_location
        {
            set;
            get;
        }

        public int m_cur_seg_index
        {
            set;
            get;
        }

        public Pushpin m_pin = null;
        public MapPolyline m_polyline = null;
        private Map m_map = null;
        private MainWindow m_wnd = null;

        public double walking_speed
        {
            set;
            get;
        }

        public walking_timer_callback(MapPolyline polyline, Map map, MainWindow wnd)
        {
            m_polyline = polyline;
            m_map = map;
            set_route(polyline);
            m_wnd = wnd;
        }

        public static double distance_on_loc(Location loc1, Location loc2)
        {
            return distance_on_geoid(loc1.Latitude,
                    loc1.Longitude,
                    loc2.Latitude,
                    loc2.Longitude);
        }

        public static double distance_on_geoid(double lat1, double lon1, double lat2, double lon2)
        {
            // Convert degrees to radians
            lat1 = lat1 * Math.PI / 180.0;
            lon1 = lon1 * Math.PI / 180.0;

            lat2 = lat2 * Math.PI / 180.0;
            lon2 = lon2 * Math.PI / 180.0;

            // radius of earth in metres
            double r = 6378100;

            // P
            double rho1 = r * Math.Cos(lat1);
            double z1 = r * Math.Sin(lat1);
            double x1 = rho1 * Math.Cos(lon1);
            double y1 = rho1 * Math.Sin(lon1);

            // Q
            double rho2 = r * Math.Cos(lat2);
            double z2 = r * Math.Sin(lat2);
            double x2 = rho2 * Math.Cos(lon2);
            double y2 = rho2 * Math.Sin(lon2);

            // Dot product
            double dot = (x1 * x2 + y1 * y2 + z1 * z2);
            double cos_theta = dot / (r * r);

            double theta = Math.Acos(cos_theta);

            // Distance in Metres
            return r * theta;
        }

        // radius of earth
        const double radius = 6378100;
        public static double Bearing(Location pt1, Location pt2)
        {
            double x = Math.Cos(DegreesToRadians(pt1.Latitude))
                * Math.Sin(DegreesToRadians(pt2.Latitude))
                - Math.Sin(DegreesToRadians(pt1.Latitude))
                * Math.Cos(DegreesToRadians(pt2.Latitude))
                * Math.Cos(DegreesToRadians(pt2.Longitude - pt1.Longitude));

            double y = Math.Sin(DegreesToRadians(pt2.Longitude - pt1.Longitude))
                * Math.Cos(DegreesToRadians(pt2.Latitude));

            // Math.Atan2 can return negative value, 0 <= output value < 2*PI expected 
            return (Math.Atan2(y, x) + Math.PI * 2) % (Math.PI * 2);
        }

        public static double DegreesToRadians(double angle)
        {
            return angle * Math.PI / 180.0d;
        }

        public static double RadiansToDegrees(double radians)
        {
            const double radToDegFactor = 180 / Math.PI;
            return radians * radToDegFactor;
        }

        public static Location FindPointAtDistanceFrom(Location startPoint,
            double bearing, double distance)
        {

            var distRatio = distance / radius;
            var distRatioSine = Math.Sin(distRatio);
            var distRatioCosine = Math.Cos(distRatio);

            var startLatRad = DegreesToRadians(startPoint.Latitude);
            var startLonRad = DegreesToRadians(startPoint.Longitude);

            var startLatCos = Math.Cos(startLatRad);
            var startLatSin = Math.Sin(startLatRad);

            var endLatRads = Math.Asin((startLatSin * distRatioCosine)
                + (startLatCos * distRatioSine * Math.Cos(bearing)));

            var endLonRads = startLonRad
                + Math.Atan2(
                    Math.Sin(bearing) * distRatioSine * startLatCos,
                    distRatioCosine - startLatSin * Math.Sin(endLatRads));

            return new Location
            {
                Latitude = RadiansToDegrees(endLatRads),
                Longitude = RadiansToDegrees(endLonRads)
            };
        }

        /// <summary>
        /// calculate the next step.
        /// </summary>
        /// <returns></returns>
        public Location get_next_step_location()
        {
            Location next_location = new Location();
            double dis_to_next_seg = distance_on_loc(m_cur_location,
                m_polyline.Locations[m_cur_seg_index + 1]);

            // check if the potential next step is out of 
            // the range of current segment.
            double dis_walk_500ms = walking_speed / 2;

            if (dis_walk_500ms < dis_to_next_seg)
            {
                // current segment.
                double bearing = Bearing(m_polyline.Locations[m_cur_seg_index],
                    m_polyline.Locations[m_cur_seg_index + 1]);

                next_location = FindPointAtDistanceFrom(m_cur_location,
                    bearing,
                    dis_walk_500ms);
            }
            else
            {
                // move to the next segment.
                m_cur_seg_index++;

                // the end of whole track.
                if (m_cur_seg_index >= m_polyline.Locations.Count() - 1)
                {
                    m_cur_seg_index = 0;

                    // reverse the walk AB --> BA -->AB.
                    for (int i = 0; i < m_polyline.Locations.Count; i++)
                        m_polyline.Locations.Move(m_polyline.Locations.Count - 1, i);

                    return m_polyline.Locations[0];
                }

                double mode_dis = dis_walk_500ms - dis_to_next_seg;
                double bearing = Bearing(m_polyline.Locations[m_cur_seg_index],
                    m_polyline.Locations[m_cur_seg_index + 1]);

                next_location = FindPointAtDistanceFrom(
                    m_polyline.Locations[m_cur_seg_index],
                    bearing,
                    mode_dis);
            }

            return next_location;
        }

        /// <summary>
        /// the timer callback -- walk one step
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void one_step(object sender, EventArgs e)
        {
            if (m_pin == null)
            {
                m_pin = new Pushpin();
            }

            m_pin.Location = m_cur_location;
            m_map.Children.Remove(m_pin);

            m_cur_location = get_next_step_location();

            if ((bool)m_wnd.gps_drift.IsChecked)
            {
                // drift the GPS (lon, lat)
                Random rnd = new Random();
                double lon_drift = rnd.NextDouble() * (0.00004 - 0.00001) + 0.00001;
                double lat_drift = rnd.NextDouble() * (0.00004 - 0.00001) + 0.00001;
                int direction = (rnd.Next(0, 1) > 0) ? 1 : -1;

                m_pin.Location = new Location(
                    m_cur_location.Latitude + lat_drift * direction,
                    m_cur_location.Longitude + lon_drift * direction,
                    // add elevation data if GPX has it.
                    m_polyline.Locations[m_cur_seg_index].Latitude);
            }
            else
            {
                m_pin.Location = m_cur_location;
            }

            m_map.Children.Add(m_pin);

            // update the location to device
            location_service.GetInstance(m_wnd).UpdateLocation(m_pin.Location);
        }

        /// <summary>
        /// reset to the beginning of the route.
        /// </summary>
        public void reset()
        {
            m_map.Children.Remove(m_pin);
            set_route(m_polyline);
        }

        /// <summary>
        /// set route to walk
        /// </summary>
        /// <param name="polyline"></param>
        public void set_route(MapPolyline polyline)
        {
            m_polyline = polyline;

            // initialize from beginning
            if (m_polyline != null)
            {
                m_cur_location = m_polyline.Locations[0];
                m_cur_seg_index = 0;
            }
        }
    }
}
