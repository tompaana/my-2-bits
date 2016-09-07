using CommunicationProtocol;
using ProcessMonitoring;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ProcessMonitorUwp
{
    /// <summary>
    /// The main page of the app.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private AppServiceBridgeManager _appServiceBridgeManager;
        private EventBasedProcessMonitorUniversal _processMonitor;

        public MainPage()
        {
            this.InitializeComponent();
            _appServiceBridgeManager = new AppServiceBridgeManager();
            _processMonitor = new EventBasedProcessMonitorUniversal();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                await _appServiceBridgeManager.LaunchBackgroundProcessAsync();
            }
            catch (Exception ex)
            {
                MessageDialog messageDialog =
                    new MessageDialog("Failed to launch the background process: " + ex.Message);
                await messageDialog.ShowAsync();
            }

            _processMonitor.ProcessStateChanged += OnProcessStateChangedAsync;
            _processMonitor.StartListeningForWindowChanges();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _processMonitor.ProcessStateChanged -= OnProcessStateChangedAsync;
            _processMonitor.StopListeningForWindowChanges();
            base.OnNavigatedFrom(e);
        }

        private async void OnProcessStateChangedAsync(object sender, ProcessStateChangedEventArgs e)
        {
            if (_appServiceBridgeManager.BackgroundProcessLaunched)
            {
                string responseValue = await _appServiceBridgeManager.SendRequestToBackgroundProcessAsync(
                    Keys.KeyProcessDetailsByWindowHandleRequest, e.ProcessProxy.MainWindowHandle.ToString());

                ProcessProxy processProxy = ProcessProxy.FromJson(responseValue);

                if (processProxy != null)
                {
                    System.Diagnostics.Debug.WriteLine("OnProcessStateChangedAsync: " + e + " " + processProxy);
                    logControl.AddLogMessage("Process state changed: " + e.ToString() + " " + processProxy.ToString());
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("OnProcessStateChangedAsync: " + e);
                logControl.AddLogMessage("Process state changed: " + e.ToString());
            }
        }
    }
}
