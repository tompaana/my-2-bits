/*
 * SOURCE NOTICE
 * 
 * The code in this file was adopted from stack overflow answer by James
 * (http://stackoverflow.com/users/82586/james). The original stack overflow discussion -
 * as of writing this - can found at http://stackoverflow.com/questions/17222788/fire-event-when-user-changes-active-process
 */

using System;
using System.Runtime.InteropServices;

namespace ProcessMonitoring
{
    /// <summary>
    /// Note that in order to use this class you need to have an event loop in place.
    /// For instance, this class cannot function in a basic console app.
    /// The code is otherwise universal and will work with both Win32 and UWP apps.
    /// </summary>
    public class EventBasedProcessMonitorUniversal
    {
        public event EventHandler<ProcessStateChangedEventArgs> ProcessStateChanged;

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventProc lpfnWinEventProc, int idProcess, int idThread, uint dwflags);
        [DllImport("user32.dll")]
        internal static extern int UnhookWinEvent(IntPtr hWinEventHook);
        internal delegate void WinEventProc(IntPtr hWinEventHook, uint iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime);

        // See https://msdn.microsoft.com/en-us/library/windows/desktop/dd318066(v=vs.85).aspx
        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        private static EventBasedProcessMonitorUniversal _eventBasedProcessMonitor;
        private WinEventProc _listener;
        private IntPtr _winHook;

        public EventBasedProcessMonitorUniversal()
        {
            _eventBasedProcessMonitor = this;
        }

        public void StartListeningForWindowChanges()
        {
            _listener = new WinEventProc(EventCallback);
            _winHook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _listener, 0, 0, WINEVENT_OUTOFCONTEXT);
            System.Diagnostics.Debug.WriteLine("StartListeningForWindowChanges: Win event hook: " + _winHook);
        }

        public void StopListeningForWindowChanges()
        {
            UnhookWinEvent(_winHook);
        }

        private static void EventCallback(
            IntPtr hWinEventHook, uint iEvent, IntPtr hWnd, int idObject, int idChild,
            int dwEventThread, int dwmsEventTime)
        {
            ProcessStateChangedEventArgs processStatedChangeEventArgs = new ProcessStateChangedEventArgs();
            processStatedChangeEventArgs.ProcessProxy.MainWindowHandle = hWnd.ToInt32();
            processStatedChangeEventArgs.EventType = (int)iEvent;

            if (processStatedChangeEventArgs.EventType == (int)EVENT_SYSTEM_FOREGROUND)
            {
                processStatedChangeEventArgs.StateChangeType =
                    ProcessStateChangedEventArgs.StateChangeTypes.BroughtForeground;
            }

            _eventBasedProcessMonitor?.ProcessStateChanged?.Invoke(_eventBasedProcessMonitor, processStatedChangeEventArgs);
        }
    }
}
