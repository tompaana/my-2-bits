using System;
using Windows.ApplicationModel.AppService;

namespace ProcessMonitorConsole
{
    public class AppServiceConnectionManager
    {
        private const string AppServiceName = "CommunicationService";
        private static AppServiceConnectionManager _appServiceConnectionManager;
        private AppServiceConnection _appServiceConnection;
        private AppServiceConnectionRequestHandler _appServiceConnectionRequestHandler;

        private AppServiceConnectionManager()
        {
            _appServiceConnectionRequestHandler = new AppServiceConnectionRequestHandler();
        }

        /// <summary>
        /// Creates the app service connection
        /// </summary>
        public static async void ThreadProc()
        {
            if (_appServiceConnectionManager == null)
            {
                _appServiceConnectionManager = new AppServiceConnectionManager();
                _appServiceConnectionManager._appServiceConnection = new AppServiceConnection();
                _appServiceConnectionManager._appServiceConnection.AppServiceName = AppServiceName;
            }

            try
            {
                _appServiceConnectionManager._appServiceConnection.PackageFamilyName =
                    Windows.ApplicationModel.Package.Current.Id.FamilyName;
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to start the app service connection thread: " + e.Message);
                return;
            }

            _appServiceConnectionManager._appServiceConnection.RequestReceived +=
                _appServiceConnectionManager._appServiceConnectionRequestHandler.OnAppServiceConnectionRequestReceived;

            AppServiceConnectionStatus status =
                await _appServiceConnectionManager._appServiceConnection.OpenAsync();

            switch (status)
            {
                case AppServiceConnectionStatus.Success:
                    Logger.Log("Connection established - waiting for requests...", ConsoleColor.Magenta);
                    break;
                case AppServiceConnectionStatus.AppNotInstalled:
                    Logger.LogError("The app AppServicesProvider is not installed");
                    break;
                case AppServiceConnectionStatus.AppUnavailable:
                    Logger.LogError("The app AppServicesProvider is not available");
                    break;
                case AppServiceConnectionStatus.AppServiceUnavailable:
                    Logger.LogError(string.Format("The app AppServicesProvider is installed but it does not provide the app service {0}",
                        _appServiceConnectionManager._appServiceConnection.AppServiceName));
                    break;
                case AppServiceConnectionStatus.Unknown:
                    Logger.LogError(string.Format("An unkown error occurred while we were trying to open an AppServiceConnection"));
                    break;
            }
        }
    }
}
