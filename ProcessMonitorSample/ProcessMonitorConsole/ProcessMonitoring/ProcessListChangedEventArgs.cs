using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ProcessMonitoring
{
    public class ProcessListChangedEventArgs : EventArgs
    {
        public IList<Process> AddedProcesses
        {
            get;
            set;
        }

        public IList<Process> RemovedProcesses
        {
            get;
            set;
        }

        public int NewProcessCount
        {
            get;
            set;
        }

        public int PreviousProcessCount
        {
            get;
            set;
        }

        public ProcessListChangedEventArgs()
        {
            AddedProcesses = new List<Process>();
            RemovedProcesses = new List<Process>();
        }
    }
}
