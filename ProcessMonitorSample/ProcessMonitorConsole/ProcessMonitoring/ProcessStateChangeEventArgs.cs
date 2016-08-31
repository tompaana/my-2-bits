using System;

namespace ProcessMonitoring
{
    public class ProcessStateChangeEventArgs : EventArgs
    {
        public int WindowHandle
        {
            get;
            set;
        }

        public int EventType
        {
            get;
            set;
        }

        public override string ToString()
        {
            return "[Window handle: " + WindowHandle + ", Event type: " + EventType + "]";
        }
    }
}
