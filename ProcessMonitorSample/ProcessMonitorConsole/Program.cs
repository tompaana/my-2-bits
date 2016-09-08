using ProcessMonitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessMonitorConsole
{
    class Program
    {
        private const int PollingFrequencyInMilliseconds = 200;

        static void Main(string[] args)
        {
            Logger.Log("--- Process Monitor console ---");

            Thread appServiceThread = new Thread(new ThreadStart(AppServiceConnectionManager.ThreadProc));
            appServiceThread.Start();

            PollingProcessMonitor processMonitor = new PollingProcessMonitor();
            ProcessMonitorEventHandler processMonitorEventHandler = new ProcessMonitorEventHandler(processMonitor);

            while (true)
            {
                processMonitor.UpdateProcessList();
                Thread.Sleep(PollingFrequencyInMilliseconds);
            }
        }
    }
}
