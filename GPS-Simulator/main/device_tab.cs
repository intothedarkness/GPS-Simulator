
//
//  Created by Richard Zhang (Richard.Rupo.Zhang@gmail.com) on 3/2020
//  Copyright © 2020 Richard Zhang. All rights reserved.
//

// Bing Map WPF control
using System.Windows;

namespace GPS_Simulator
{
    public partial class MainWindow : Window
    {
        private void device_Button_Click(object sender, RoutedEventArgs e)
        {
            // only take care of the first device.
            if (location_service.GetInstance(this).Devices.Count > 1)
            {
                System.Windows.Forms.MessageBox.Show("More than one device is connected, provision tool only support one device at a time!");
                return;
            }

            dev_prov dlg = new dev_prov(this, location_service.GetInstance(this).Devices);
            dlg.ShowDialog();
        }
    }
}