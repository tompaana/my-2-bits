using System.Collections.ObjectModel;

#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else
using System.Windows.Controls;
#endif

namespace ProcessMonitorUwp.Logging
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
            this.InitializeComponent();
            LogItems = new ObservableCollection<LogItem>();
        }

        public void AddLogMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                LogItem logItem = new LogItem(message);
                LogItems.Add(logItem);
            }
        }
    }
}
