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
        static void Main(string[] args)
        {
            Logger.Log("--- Process monitor console ---");

            Thread appServiceThread = new Thread(new ThreadStart(AppServiceConnectionManager.ThreadProc));
            appServiceThread.Start();

            PollingProcessMonitor processMonitor = new PollingProcessMonitor();
            ProcessMonitorEventHandler processMonitorEventHandler = new ProcessMonitorEventHandler(processMonitor);

            while (true)
            {
                processMonitor.UpdateProcessList();
                Thread.Sleep(200);
            }
        }
    }
}
