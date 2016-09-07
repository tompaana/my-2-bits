using System.Collections.ObjectModel;

#if WINDOWS_UWP
using System;
using Windows.UI.Xaml.Controls;
#else
using System.Windows.Controls;
#endif

namespace ProcessMonitor.Logging
{
    public sealed partial class LogControl : UserControl
    {
        public ObservableCollection<LogItem> LogItems
        {
            get;
            private set;
        }

        public LogControl()
        {
            LogItems = new ObservableCollection<LogItem>();
            this.InitializeComponent();
        }

#if WINDOWS_UWP
        public async void AddLogMessage(string message)
#else
        public void AddLogMessage(string message)
#endif
        {
            if (!string.IsNullOrEmpty(message))
            {
                LogItem logItem = new LogItem(message);
#if WINDOWS_UWP
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                    Windows.UI.Core.CoreDispatcherPriority.Normal, () => LogItems.Add(logItem));
#else
                Dispatcher.InvokeAsync(() => LogItems.Add(logItem));
#endif
            }
        }
    }
}
