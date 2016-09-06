#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else
using System.Windows.Controls;
#endif

namespace ProcessMonitorUwp.Logging
{
    public sealed partial class LogItemControl : UserControl
    {
        public LogItemControl()
        {
            this.InitializeComponent();
        }
    }
}
