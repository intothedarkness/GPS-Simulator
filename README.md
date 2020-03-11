# GPS-Simulator
GPS-Simulator is Windows app which allows you to spoofing a location on iOS device.

This app using [imobiledevice-net](https://github.com/libimobiledevice-win32/imobiledevice-net) library and bingfor spoofing a location on device.

## Features

- [x] Spoofing iOS device location without jailbrake or app install.
- [x] Easy to set device location from the map, just by double clicking the map
- [x] Supported GPX track auto walking with 3 different speed.

### Preview

## How to Build

### Requirements

- Windows 10
- Visual Studio 2019
- .NET framework 4.7
- Download and install the Bing Maps Windows Presentation Foundation Control (https://www.microsoft.com/en-us/download/details.aspx?id=27165)
- Download and install the Windows Presentation Foundation Control SDK. (WPF)

### Build the app

1. Open the GPS-Simulator.sln from VisualStudio
2. Install latest version of imobiledevice-net as a NuGet package

PM> Install-Package imobiledevice-net

3. add reference of Bin Maps WPF control


In the Add Reference dialog box, click the Browse tab.

Browse to the location of the Bing Maps WPF Control installation. Typically, the control is installed in the Program Files or Program Files (x86) folders on the drive that contains your operating system. Open the Libraries folder, select the Microsoft.Maps.MapControl.WPF.dll file and then click OK. 

4. Click the build


## Usage

- Start spoofing

  1. Connect the iOS device to your computer. (ensure your device is privisoned correctly)
  2. Install Traditional win32 version iTunes Itunes (Note the version from Windows Store)
  3. Double click on the map to your desired location.

- Auto walking
  1. Load a GPX track file
  2. Set the walking speed (fast walking / running / driving)
  3. Click start

  Note: it will auto repeat the track.

- Provsion the device
  1. Click the more button in "Device" section.
  2. Click start to auto privision your device.
  
  
  Note: This feature is still under development it works for some devices, it might have bugs.
  if you have trouble to provision the device, try to use the manual way as below:

  1. download the correct DeveloperDiskImage.dmg and DeveloperDiskImage.dmg.signature file from Apple website (in macOS Xcode will         automatically download it for your device, you can get from there if you have a Mac)
  2. connect your device and unlock the device (ensure the device is showing up in the iTunes)
  3. use command 
        "ideviceimagemounter.exe DeveloperDiskImage.dmg"

  


## Acknowledgements

GPS-Simulator uses the following libraries:
- [imobiledevice-net](https://github.com/libimobiledevice-win32/imobiledevice-net)

