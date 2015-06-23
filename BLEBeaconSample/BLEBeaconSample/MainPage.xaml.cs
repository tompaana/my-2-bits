using System;
using System.Collections.ObjectModel;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace BLEBeaconSample
{
    public sealed partial class MainPage : Page
    {
        #region Constants

        // Set the values below (manufacturer ID and beacon code) to filter the beacons
        // based on the manufacturer.
        private const UInt16 ManufacturerId = 0x0012;
        private const UInt16 BeaconCode = 0xBEAC;

        private const string DefaultBeaconId1 = "abcdef12-3456-7890-abcd-ef1234567890";
        private const string DefaultBeaconId2 = "1234";
        private const string DefaultBeaconId3 = "5678";

        #endregion

        #region Properties for XAML UI

        public bool IsWatcherStarted
        {
            get
            {
                return (bool)GetValue(IsWatcherStartedProperty);
            }
            private set
            {
                SetValue(IsWatcherStartedProperty, value);
            }
        }
        public static readonly DependencyProperty IsWatcherStartedProperty =
            DependencyProperty.Register("IsWatcherStarted", typeof(bool), typeof(MainPage),
                new PropertyMetadata(false));

        public bool IsPublisherStarted
        {
            get
            {
                return (bool)GetValue(IsPublisherStartedProperty);
            }
            private set
            {
                SetValue(IsPublisherStartedProperty, value);
            }
        }
        public static readonly DependencyProperty IsPublisherStartedProperty =
            DependencyProperty.Register("IsPublisherStarted", typeof(bool), typeof(MainPage),
                new PropertyMetadata(false));

        public string BeaconId1
        {
            get
            {
                return (string)GetValue(BeaconId1Property);
            }
            private set
            {
                SetValue(BeaconId1Property, value);
            }
        }
        public static readonly DependencyProperty BeaconId1Property =
            DependencyProperty.Register("BeaconId1", typeof(string), typeof(MainPage),
                new PropertyMetadata(DefaultBeaconId1));

        public string BeaconId2
        {
            get
            {
                return (string)GetValue(BeaconId2Property);
            }
            private set
            {
                SetValue(BeaconId2Property, value);
            }
        }
        public static readonly DependencyProperty BeaconId2Property =
            DependencyProperty.Register("BeaconId2", typeof(string), typeof(MainPage),
                new PropertyMetadata(DefaultBeaconId2));

        public string BeaconId3
        {
            get
            {
                return (string)GetValue(BeaconId3Property);
            }
            private set
            {
                SetValue(BeaconId3Property, value);
            }
        }
        public static readonly DependencyProperty BeaconId3Property =
            DependencyProperty.Register("BeaconId3", typeof(string), typeof(MainPage),
                new PropertyMetadata(DefaultBeaconId3));

        #endregion

        public MainPage()
        {
            this.InitializeComponent();

            // Initialize the scanner and the advertiser
            InitializeScanner();
            InitializeAdvertiser();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Start the advertisement watcher when the page is shown
            try
            {
                _bluetoothLEAdvertisemenetWatcher.Start();

                IsWatcherStarted = true;
                toggleWatcherButton.IsChecked = true;
            }
            catch (Exception ex)
            {
                MessageDialog messageDialog = new MessageDialog("Failed to start Bluetooth LE Advertisement Watcher: " + ex.Message);
                await messageDialog.ShowAsync();
            }
        }

        #region Scanner

        public ObservableCollection<Beacon> BeaconCollection
        {
            get;
            set;
        }

        private BluetoothLEAdvertisementWatcher _bluetoothLEAdvertisemenetWatcher;
        
        /// <summary>
        /// Constructs the collection for beacons that we detect and the BLE beacon advertisement
        /// watcher.
        /// 
        /// Hooks the events of the watcher and sets the watcher filter based on
        /// the manufacturer ID and beacon code.
        /// </summary>
        private void InitializeScanner()
        {
            BeaconCollection = new ObservableCollection<Beacon>();

            _bluetoothLEAdvertisemenetWatcher = new BluetoothLEAdvertisementWatcher();

            _bluetoothLEAdvertisemenetWatcher.Stopped += OnWatcherStoppedAsync;
            _bluetoothLEAdvertisemenetWatcher.Received += OnAdvertisemenetReceivedAsync;

            BluetoothLEManufacturerData manufacturerData = BeaconFactory.BeaconManufacturerData(ManufacturerId, BeaconCode);

            _bluetoothLEAdvertisemenetWatcher.AdvertisementFilter.Advertisement.ManufacturerData.Add(manufacturerData);
        }

        private async void OnWatcherStoppedAsync(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementWatcherStoppedEventArgs args)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    System.Diagnostics.Debug.WriteLine("Bluetooth LE Advertisement Watcher stopped with status: "
                        + _bluetoothLEAdvertisemenetWatcher.Status);
                    IsWatcherStarted = false;

                    if (_bluetoothLEAdvertisemenetWatcher.Status == BluetoothLEAdvertisementWatcherStatus.Aborted)
                    {
                        // Aborted status most likely means that Bluetooth is not enabled (i.e. turned off)
                        MessageDialog messageDialog = new MessageDialog("Bluetooth LE Advertisement Watcher aborted. Make sure you have Bluetooth turned on on your device and try again.");
                        await messageDialog.ShowAsync();
                    }
                });
        }

        private async void OnAdvertisemenetReceivedAsync(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    Beacon beacon = BeaconFactory.BeaconFromBluetoothLEAdvertisementReceivedEventArgs(args);

                    if (beacon != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Received advertisement from beacon " + beacon.ToString());
                        bool existingBeaconUpdated = false;

                        foreach (Beacon existingBeacon in BeaconCollection)
                        {
                            if (existingBeacon.Update(beacon))
                            {
                                existingBeaconUpdated = true;
                            }
                        }

                        if (!existingBeaconUpdated)
                        {
                            BeaconCollection.Add(beacon);
                        }
                    }
                });
        }

        private async void OnToggleWatcherButtonClickedAsync(object sender, RoutedEventArgs e)
        {
            if (_bluetoothLEAdvertisemenetWatcher.Status != BluetoothLEAdvertisementWatcherStatus.Started)
            {
                try
                {
                    _bluetoothLEAdvertisemenetWatcher.Start();

                    IsWatcherStarted = true;
                    toggleWatcherButton.IsChecked = true;
                }
                catch (Exception ex)
                {
                    MessageDialog messageDialog = new MessageDialog("Failed to start Bluetooth LE Advertisement Watcher: " + ex.Message);
                    await messageDialog.ShowAsync();
                }
            }
            else
            {
                _bluetoothLEAdvertisemenetWatcher.Stop();
                toggleWatcherButton.IsChecked = false;
                BeaconCollection.Clear();
            }
        }

        #endregion

        #region Advertiser

        private BluetoothLEAdvertisementPublisher _bluetoothLEAdvertisementPublisher;

        private void InitializeAdvertiser()
        {
            _bluetoothLEAdvertisementPublisher = new BluetoothLEAdvertisementPublisher();
            _bluetoothLEAdvertisementPublisher.StatusChanged += OnPublisherStatusChangedAsync;
        }

        private async void OnPublisherStatusChangedAsync(BluetoothLEAdvertisementPublisher sender, BluetoothLEAdvertisementPublisherStatusChangedEventArgs args)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    System.Diagnostics.Debug.WriteLine("Bluetooth LE Advertisement Publisher status changed to "
                        + args.Status);

                    if (args.Status == BluetoothLEAdvertisementPublisherStatus.Aborted)
                    {
                        // Aborted status most likely means that Bluetooth is not enabled (i.e. turned off)
                        MessageDialog messageDialog = new MessageDialog("Bluetooth LE Advertisement Publisher aborted. Make sure you have Bluetooth turned on on your device and try again.");
                        await messageDialog.ShowAsync();
                    }

                    IsPublisherStarted = (args.Status == BluetoothLEAdvertisementPublisherStatus.Started);
                });
        }

        private async void OnTogglePublisherButtonClickedAsync(object sender, RoutedEventArgs e)
        {
            if (_bluetoothLEAdvertisementPublisher.Status != BluetoothLEAdvertisementPublisherStatus.Started)
            {
                Beacon beacon = new Beacon();
                beacon.ManufacturerId = ManufacturerId;
                beacon.Code = BeaconCode;
                beacon.Id1 = BeaconId1;

                try
                {
                    beacon.Id2 = UInt16.Parse(BeaconId2);
                    beacon.Id3 = UInt16.Parse(BeaconId3);
                }
                catch (Exception)
                {
                }

                beacon.MeasuredPower = -58;

                System.Diagnostics.Debug.WriteLine("Will try to advertise as beacon " + beacon.ToString());

                BluetoothLEAdvertisementDataSection dataSection = BeaconFactory.BeaconToSecondDataSection(beacon);
                _bluetoothLEAdvertisementPublisher.Advertisement.DataSections.Add(dataSection);

                try
                {
                    _bluetoothLEAdvertisementPublisher.Start();
                }
                catch (Exception ex)
                {
                    MessageDialog messageDialog = new MessageDialog("Failed to start Bluetooth LE Advertisement Publisher: " + ex.Message);
                    await messageDialog.ShowAsync();
                }
            }
            else
            {
                _bluetoothLEAdvertisementPublisher.Stop();
            }
        }

        private void OnAdvertisingTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox textBox = sender as TextBox;
                string textBoxName = textBox.Name.ToLower();
                string text = textBox.Text;

                if (textBoxName.StartsWith("beaconid1"))
                {
                    int oldTextLength = text.Length;
                    int oldCaretPosition = textBox.SelectionStart;

                    BeaconId1 = BeaconFactory.FormatUuid(text);

                    int newCaretPosition = oldCaretPosition + (BeaconId1.Length - oldTextLength);

                    if (newCaretPosition > 0 && newCaretPosition <= BeaconId1.Length)
                    {
                        textBox.SelectionStart = newCaretPosition;
                    }
                }
                else if (textBoxName.StartsWith("beaconid2"))
                {
                    BeaconId2 = text;
                }
                else if (textBoxName.StartsWith("beaconid3"))
                {
                    BeaconId3 = text;
                }
            }
        }

        #endregion
    }
}
