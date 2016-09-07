using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ProcessMonitoring
{
    /// <summary>
    /// A process monitor that can be used to detect created and removed processes as well as
    /// the change of the top-most (foreground) process.
    /// 
    /// This monitor is used in polling manner so the frequency of events depend on how ofter
    /// UpdateProcessList method is called.
    /// 
    /// The code is compatible with Win32 apps.
    /// </summary>
    public class PollingProcessMonitor
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

        public event EventHandler<ProcessListChangedEventArgs> ProcessListChanged;
        public event EventHandler<ForegroundProcessChangedEventArgs> ForegroundProcessChanged;

        private Process[] _previousProcesses = null;
        private Process _previousForegroundProcess = null;

        public PollingProcessMonitor()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool UpdateProcessList()
        {
            ProcessListChangedEventArgs processListChangedEventArgs =
                    new ProcessListChangedEventArgs();
            Process[] currentProcesses = Process.GetProcesses();

            if (_previousProcesses == null)
            {
                processListChangedEventArgs.PreviousProcessCount = 0;

                if (currentProcesses != null)
                {
                    processListChangedEventArgs.NewProcessCount = currentProcesses.Length;
                    processListChangedEventArgs.AddedProcesses = currentProcesses;
                }
            }
            else if (currentProcesses == null)
            {
                processListChangedEventArgs.NewProcessCount = 0;

                if (_previousProcesses != null)
                {
                    processListChangedEventArgs.PreviousProcessCount = _previousProcesses.Length;
                    processListChangedEventArgs.RemovedProcesses = _previousProcesses;
                }
            }
            else // Neither of the lists is null
            {
                processListChangedEventArgs.PreviousProcessCount = _previousProcesses.Length;
                processListChangedEventArgs.NewProcessCount = currentProcesses.Length;

                IDictionary<int, Process> previousProcessesDictionary = new SortedDictionary<int, Process>();
                IDictionary<int, Process> currentProcessesDictionary = new SortedDictionary<int, Process>();

                for (int i = 0; i < _previousProcesses.Length; ++i)
                {
                    previousProcessesDictionary.Add(_previousProcesses[i].Id, _previousProcesses[i]);
                }

                for (int i = 0; i < currentProcesses.Length; ++i)
                {
                    currentProcessesDictionary.Add(currentProcesses[i].Id, currentProcesses[i]);
                }

                Process process = null;

                foreach (int id in previousProcessesDictionary.Keys)
                {
                    if (!currentProcessesDictionary.ContainsKey(id))
                    {
                        previousProcessesDictionary.TryGetValue(id, out process);

                        if (process != null)
                        {
                            processListChangedEventArgs.RemovedProcesses.Add(process);
                        }
                    }
                }

                foreach (int id in currentProcessesDictionary.Keys)
                {
                    if (!previousProcessesDictionary.ContainsKey(id))
                    {
                        currentProcessesDictionary.TryGetValue(id, out process);

                        if (process != null)
                        {
                            processListChangedEventArgs.AddedProcesses.Add(process);
                        }
                    }
                }
            }

            if (currentProcesses != null && ForegroundProcessChanged != null)
            {
                // Check if the previous foreground process is the same
                Process process = null;
                IntPtr foregroundWindowHandle = GetForegroundWindow();
                bool foregroundProcessFound = false;

                for (int i = 0; i < currentProcesses.Length; ++i)
                {
                    process = currentProcesses[i];

                    if (process.MainWindowHandle == foregroundWindowHandle)
                    {
                        foregroundProcessFound = true;

                        if (_previousForegroundProcess == null
                            || _previousForegroundProcess.Id != process.Id)
                        {
                            ForegroundProcessChangedEventArgs foregroundProcessChangedEventArgs =
                                new ForegroundProcessChangedEventArgs();
                            foregroundProcessChangedEventArgs.NewForegroundProcess = process;
                            foregroundProcessChangedEventArgs.PreviousForegroundProcess =
                                _previousForegroundProcess;

                            ForegroundProcessChanged(this, foregroundProcessChangedEventArgs);

                            _previousForegroundProcess = process;
                        }
                    }
                }

                if (!foregroundProcessFound && _previousForegroundProcess != null)
                {
                    ForegroundProcessChangedEventArgs foregroundProcessChangedEventArgs =
                        new ForegroundProcessChangedEventArgs();
                    foregroundProcessChangedEventArgs.NewForegroundProcess = null;
                    foregroundProcessChangedEventArgs.PreviousForegroundProcess =
                        _previousForegroundProcess;

                    ForegroundProcessChanged(this, foregroundProcessChangedEventArgs);

                    _previousForegroundProcess = null;
                }
            }

            _previousProcesses = currentProcesses;

            if (processListChangedEventArgs.AddedProcesses.Count > 0
                || processListChangedEventArgs.RemovedProcesses.Count > 0)
            {
                ProcessListChanged?.Invoke(this, processListChangedEventArgs);
                return true;
            }

            return false;
        }
    }
}
