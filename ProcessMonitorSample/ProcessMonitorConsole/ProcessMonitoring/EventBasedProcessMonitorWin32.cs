using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using WMI.Win32;

namespace ProcessMonitoring
{
    public class EventBasedProcessMonitorWin32 : ManagementEventWatcher
    {
        public event EventHandler<ProcessStateChangedEventArgs> ProcessStateChanged;

        // WMI WQL process query strings
        static readonly string WMI_OPER_EVENT_QUERY = @"SELECT * FROM 
__InstanceOperationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process'";
        static readonly string WMI_OPER_EVENT_QUERY_WITH_PROC =
            WMI_OPER_EVENT_QUERY + " and TargetInstance.Name = '{0}'";

        public EventBasedProcessMonitorWin32()
        {
            Init(string.Empty);
        }
        public EventBasedProcessMonitorWin32(string processName)
        {
            Init(processName);
        }
        private void Init(string processName)
        {
            Query.QueryLanguage = "WQL";

            if (string.IsNullOrEmpty(processName))
            {
                Query.QueryString = WMI_OPER_EVENT_QUERY;
            }
            else
            {
                Query.QueryString =
                    string.Format(WMI_OPER_EVENT_QUERY_WITH_PROC, processName);
            }

            EventArrived += new EventArrivedEventHandler(OnEventArrived);
        }

        private void OnEventArrived(object sender, EventArrivedEventArgs e)
        {
            string eventType = e.NewEvent.ClassPath.ClassName;
            
            Win32Process process = new
                Win32Process(e.NewEvent["TargetInstance"] as ManagementBaseObject);

            if (ProcessStateChanged != null)
            {
                ProcessStateChangedEventArgs processStateChangedEventArgs = new ProcessStateChangedEventArgs();
                processStateChangedEventArgs.ProcessProxy.Id = (int)process.ProcessId;
                processStateChangedEventArgs.ProcessProxy.ProcessName = process.Name;

                switch (eventType)
                {
                    case "__InstanceCreationEvent":
                        processStateChangedEventArgs.StateChangeType =
                            ProcessStateChangedEventArgs.StateChangeTypes.Created;
                        break;
                    case "__InstanceModificationEvent":
                        processStateChangedEventArgs.StateChangeType =
                            ProcessStateChangedEventArgs.StateChangeTypes.Modified;
                        break;
                    case "__InstanceDeletionEvent":
                        processStateChangedEventArgs.StateChangeType =
                            ProcessStateChangedEventArgs.StateChangeTypes.Deleted;
                        break;
                }

                if (processStateChangedEventArgs.StateChangeType
                        != ProcessStateChangedEventArgs.StateChangeTypes.Modified)
                {
                    ProcessStateChanged(this, processStateChangedEventArgs);
                }
            }
        }
    }
}
