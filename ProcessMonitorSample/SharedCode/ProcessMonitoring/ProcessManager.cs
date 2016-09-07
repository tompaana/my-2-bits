using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMonitoring
{
    /// <summary>
    /// http://stackoverflow.com/questions/71257/suspend-process-in-c-sharp
    /// </summary>
    public class ProcessManager
    {
        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);

        public IList<Process> KilledProcesses
        {
            get;
            private set;
        }

        public IList<Process> SuspendedProcesses
        {
            get;
            private set;
        }

        public ProcessManager()
        {
            KilledProcesses = new List<Process>();
            SuspendedProcesses = new List<Process>();
        }

        public void KillProcess(Process process)
        {
            if (process != null)
            {
                process.Kill();
                KilledProcesses.Add(process);
            }
        }

        public void RestartProcess(Process process)
        {
            if (process != null)
            {
                Process.Start(process.StartInfo);
                RemoveProcessFromList(process, KilledProcesses);
            }
        }

        /// <summary>
        /// Suspends the given process by suspending all its threads.
        /// </summary>
        /// <param name="process">The process to suspend.</param>
        /// <returns>True, if suspended. False otherwise.</returns>
        public bool SuspendProcess(Process process)
        {
            bool suspended = false;

            if (process != null && !string.IsNullOrEmpty(process.ProcessName))
            {
                foreach (ProcessThread processThread in process.Threads)
                {
                    IntPtr pOpenThread =
                        OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)processThread.Id);

                    if (pOpenThread != IntPtr.Zero)
                    {
                        SuspendThread(pOpenThread);
                    }
                }

                SuspendedProcesses.Add(process);
                suspended = true;
            }
            else
            {
                Debug.WriteLine("SuspendProcess: Process is null or has no name");
            }

            return suspended;
        }

        /// <summary>
        /// Resumes the given process by resuming all its threads.
        /// </summary>
        /// <param name="process">The process to resume.</param>
        /// <returns>True, if resumed. False otherwise.</returns>
        public bool ResumeProcess(Process process)
        {
            bool resumed = false;

            if (process != null && !string.IsNullOrEmpty(process.ProcessName))
            {
                foreach (ProcessThread processThread in process.Threads)
                {
                    IntPtr pOpenThread =
                        OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)processThread.Id);

                    if (pOpenThread != IntPtr.Zero)
                    {
                        int suspendCount = 0;

                        do
                        {
                            suspendCount = ResumeThread(pOpenThread);
                        }
                        while (suspendCount > 0);
                    }
                }

                RemoveProcessFromList(process, SuspendedProcesses);
                resumed = true;
            }
            else
            {
                Debug.WriteLine("ResumeProcess: Process is null or has no name");
            }

            return resumed;
        }

        /// <summary>
        /// Removes the given process from the given list.
        /// </summary>
        /// <param name="process">The process to remove.</param>
        /// <param name="processList">The list to remove the process from.</param>
        /// <returns>True, if found and removed. False otherwise.</returns>
        private bool RemoveProcessFromList(Process process, IList<Process> processList)
        {
            if (process != null && processList != null)
            {
                for (int i = 0; i < processList.Count; ++i)
                {
                    if (processList[i].Id == process.Id)
                    {
                        processList.RemoveAt(i);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
