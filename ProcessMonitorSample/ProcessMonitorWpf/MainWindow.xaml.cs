using ProcessMonitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProcessMonitorWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private EventBasedProcessMonitorWin32 _processMonitor;

        public MainWindow()
        {
            InitializeComponent();
            _processMonitor = new EventBasedProcessMonitorWin32();
            _processMonitor.ProcessStateChanged += OnProcessStateChanged;
            _processMonitor.Start();
        }

        private void OnProcessStateChanged(object sender, ProcessStateChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(DateTime.Now.ToLongTimeString() + " " + e);
            logControl.AddLogMessage(e.ToString());
        }
    }
}
