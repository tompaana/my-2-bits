using System;
using System.Diagnostics;

namespace ProcessMonitoring
{
    public class ForegroundProcessChangedEventArgs : EventArgs
    {
        public Process NewForegroundProcess
        {
            get;
            set;
        }

        public Process PreviousForegroundProcess
        {
            get;
            set;
        }
    }
}
