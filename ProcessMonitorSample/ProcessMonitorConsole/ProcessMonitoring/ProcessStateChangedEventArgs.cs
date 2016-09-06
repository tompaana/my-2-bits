using System;

namespace ProcessMonitoring
{
    public class ProcessStateChangedEventArgs : EventArgs
    {
        public enum StateChangeTypes
        {
            NotDefined,
            Created,
            BroughtForeground,
            Modified,
            Deleted,
        }

        public StateChangeTypes StateChangeType
        {
            get;
            set;
        }

        public int EventType
        {
            get;
            set;
        }

        public ProcessProxy ProcessProxy
        {
            get;
            set;
        }

        public ProcessStateChangedEventArgs()
        {
            StateChangeType = StateChangeTypes.NotDefined;
            ProcessProxy = new ProcessProxy();
        }

        public override string ToString()
        {
            return "[State change type: " + StateChangeType
                + (EventType == 0 ? "" : (", Event type: " + EventType))
                + ", Process details: " + ProcessProxy.ToString() + "]";
        }
    }
}
