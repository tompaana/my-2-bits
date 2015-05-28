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
        // Constants
        private const string DefaultBeaconProximityUuid = "abcdef12-3456-7890-abcd-ef1234567890";
        private const string DefaultBeaconMajor = "1234";
        private const string DefaultBeaconMinor = "5678";

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

        public string ProximityUuid
        {
            get
            {
                return (string)GetValue(ProximityUuidProperty);
            }
            private set
            {
                SetValue(ProximityUuidProperty, value);
            }
        }
        public static readonly DependencyProperty ProximityUuidProperty =
            DependencyProperty.Register("ProximityUuid", typeof(string), typeof(MainPage),
                new PropertyMetadata(DefaultBeaconProximityUuid));

        public string Major
        {
            get
            {
                return (string)GetValue(MajorProperty);
            }
            private set
            {
                SetValue(MajorProperty, value);
            }
        }
        public static readonly DependencyProperty MajorProperty =
            DependencyProperty.Register("Major", typeof(string), typeof(MainPage),
                new PropertyMetadata(DefaultBeaconMajor));

        public string Minor
        {
            get
            {
                return (string)GetValue(MinorProperty);
            }
            private set
            {
                SetValue(MinorProperty, value);
            }
        }
        public static readonly DependencyProperty MinorProperty =
            DependencyProperty.Register("Minor", typeof(string), typeof(MainPage),
                new PropertyMetadata(DefaultBeaconMinor));

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
        /// Hooks the events of the watcher and sets the filter to match iBeacon specification.
        /// </summary>
        private void InitializeScanner()
        {
            BeaconCollection = new ObservableCollection<Beacon>();

            _bluetoothLEAdvertisemenetWatcher = new BluetoothLEAdvertisementWatcher();

            _bluetoothLEAdvertisemenetWatcher.Stopped += OnWatcherStoppedAsync;
            _bluetoothLEAdvertisemenetWatcher.Received += OnAdvertisemenetReceivedAsync;

            BluetoothLEManufacturerData manufacturerData = BeaconFactory.DefaultBeaconManufacturerData();
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
                beacon.ProximityUuid = ProximityUuid;

                try
                {
                    beacon.Major = UInt16.Parse(Major);
                    beacon.Minor = UInt16.Parse(Minor);
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

                if (textBoxName.StartsWith("proximity"))
                {
                    int oldTextLength = text.Length;
                    int oldCaretPosition = textBox.SelectionStart;

                    ProximityUuid = BeaconFactory.FormatUuid(text);

                    int newCaretPosition = oldCaretPosition + (ProximityUuid.Length - oldTextLength);

                    if (newCaretPosition > 0 && newCaretPosition <= ProximityUuid.Length)
                    {
                        textBox.SelectionStart = newCaretPosition;
                    }
                }
                else if (textBoxName.StartsWith("major"))
                {
                    Major = text;
                }
                else if (textBoxName.StartsWith("minor"))
                {
                    Minor = text;
                }
            }
        }

        #endregion
    }
}
