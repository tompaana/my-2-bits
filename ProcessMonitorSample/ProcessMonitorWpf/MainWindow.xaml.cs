using ProcessMonitoring;
using System;
using System.Diagnostics;
using System.Windows;

namespace ProcessMonitorWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string NameOfProcessToTestWith = "notepad";
        private EventBasedProcessMonitorWin32 _processMonitor;
        private ProcessManagerTester _processManagerTester;

        public MainWindow()
        {
            InitializeComponent();

            _processManagerTester = new ProcessManagerTester(NameOfProcessToTestWith, logControl);

            _processMonitor = new EventBasedProcessMonitorWin32();
            _processMonitor.ProcessStateChanged += OnProcessStateChanged;
            _processMonitor.Start();
        }

        private void OnProcessStateChanged(object sender, ProcessStateChangedEventArgs e)
        {
            Debug.WriteLine(DateTime.Now.ToLongTimeString() + " " + e);
            logControl.AddLogMessage(e.ToString());

            if (e.StateChangeType == ProcessStateChangedEventArgs.StateChangeTypes.Created)
            {
                _processManagerTester.RunTest(Process.GetProcessById(e.ProcessProxy.Id));
            }
        }
    }
}
