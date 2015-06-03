BLE Beacon Sample
=================

**Description:** A sample project demonstrating how to scan for Bluetooth LE
(BLE) beacons and how to turn a Windows device into a beacon. No background
functionality included.

**Supported platforms:** Windows 10 (Technical Preview)

![Screenshot 1](https://raw.githubusercontent.com/tompaana/my-2-bits/master/BLEBeaconSample/Doc/Screenshot.png)

All the main logic is located in [MainPage.xaml.cs](https://github.com/tompaana/my-2-bits/blob/master/BLEBeaconSample/BLEBeaconSample/MainPage.xaml.cs)
file of the project. To make things easier I've created two convenient and
reusable (you're welcome) classes to manage beacons:

| Class | Description |
| ----- | ----------- |
| [Beacon](https://github.com/tompaana/my-2-bits/blob/master/BLEBeaconSample/BLEBeaconSample/Beacon.cs) | A utility class to hold the details of a detected beacon. Keeps also track on the time elapsed since the beacon was last seen. Can be used in a model for UI as-is, since the class implements `INotifyPropertyChanged`. |
| [BeaconFactory](https://github.com/tompaana/my-2-bits/blob/master/BLEBeaconSample/BLEBeaconSample/BeaconFactory.cs) | Provides methods to create `Beacon` instances from received beacon arguments and can turn a Beacon instance back to data sections, which can be used by the publisher. See methods `BeaconFromBluetoothLEAdvertisementReceivedEventArgs` and `BeaconToSecondDataSection`. |

### More on BLE beacon app development ###

* [Building Compelling Bluetooth Apps in Windows 10](https://channel9.msdn.com/Events/Build/2015/3-739) session from //build/ 2015
* The official Microsoft [beacon sample](https://github.com/Microsoft/Windows-universal-samples/tree/master/bluetoothadvertisement)
* My [blog post](http://tomipaananen.azurewebsites.net/?p=111) related to this topic