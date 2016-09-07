using System;
using System.ComponentModel;
using System.Diagnostics;

namespace ProcessMonitoring
{
    public class ProcessUtils
    {
        public static ProcessProxy ProcessToProcessProxy(Process process)
        {
            ProcessProxy processProxy = null;

            if (process != null)
            {
                processProxy = new ProcessProxy();
                processProxy.Id = process.Id;
                processProxy.MainWindowHandle = process.MainWindowHandle.ToInt32();
                processProxy.ProcessName = process.ProcessName;
                processProxy.MainWindowTitle = process.MainWindowTitle;
            }

            return processProxy;
        }

        public static ProcessProxy ProcessProxyByMainWindowHandle(IntPtr mainWindowHandle)
        {
            Process[] processes = Process.GetProcesses();

            for (int i = 0; i < processes.Length; ++i)
            {
                if (processes[i].MainWindowHandle == mainWindowHandle)
                {
                    return ProcessToProcessProxy(processes[i]);
                }
            }

            return null;
        }

        public static ProcessProxy ProcessProxyByMainWindowHandle(int mainWindowHandle)
        {
            return ProcessProxyByMainWindowHandle(new IntPtr(mainWindowHandle));
        }

        #region For debugging and logging

        public static void ListProcesses()
        {
            Process[] processes = Process.GetProcesses();
            Process process = null;
            string processIdAndName = null;

            for (int i = 0; i < processes.Length; ++i)
            {
                process = processes[i];
                processIdAndName = process.Id + " " + process.ProcessName;
                Debug.WriteLine(processIdAndName);
            }
        }

        public static string ProcessListAsString(bool hasWindowHandle)
        {
            Process[] processes = Process.GetProcesses();
            Process process = null;
            IntPtr zeroIntPtr = new IntPtr(0);
            string result = "";

            for (int i = 0; i < processes.Length; ++i)
            {
                process = processes[i];

                if ((process.MainWindowHandle == zeroIntPtr && !hasWindowHandle)
                    || (process.MainWindowHandle != zeroIntPtr && hasWindowHandle))
                {
                    result += process.Id + " " + process.ProcessName + "\n";
                }
            }

            return result;
        }

        public static void ListProcesses(bool hasWindowHandle)
        {
            Debug.Write(ProcessListAsString(hasWindowHandle));
        }

        public static void ListProcessDetails()
        {
            Process[] processes = Process.GetProcesses();

            for (int i = 0; i < processes.Length; ++i)
            {
                Debug.WriteLine("--------------------------------------------------------------------------------");

                try
                {
                    Debug.WriteLine(ProcessDetailsToString(processes[i]));
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }

            Debug.WriteLine("--------------------------------------------------------------------------------");
        }

        public static string ProcessStartInfoToString(Process process, bool includeEnvironmentVariables = false)
        {
            string processStartInfoAsString = "Start info of process " + process.ProcessName + ":\n";

            if (process.StartInfo != null)
            {
                processStartInfoAsString += "\tArguments: " + process.StartInfo.Arguments + "\n";
                processStartInfoAsString += "\tCreate no window: " + process.StartInfo.CreateNoWindow + "\n";
                processStartInfoAsString += "\tDomain: " + process.StartInfo.Domain + "\n";

                if (includeEnvironmentVariables)
                {
                    processStartInfoAsString += "\tEnvironment variables: " + process.StartInfo.EnvironmentVariables + "\n";

                    if (process.StartInfo.EnvironmentVariables != null)
                    {
                        foreach (string key in process.StartInfo.EnvironmentVariables.Keys)
                        {
                            processStartInfoAsString += "\t\t" + key + " => " + process.StartInfo.EnvironmentVariables[key] + "\n";
                        }
                    }
                }

                processStartInfoAsString += "\tError dialog: " + process.StartInfo.ErrorDialog + "\n";
                processStartInfoAsString += "\tError dialog parent handle: " + process.StartInfo.ErrorDialogParentHandle + "\n";
                processStartInfoAsString += "\tFile name: " + process.StartInfo.FileName + "\n";
                processStartInfoAsString += "\tLoad user profile: " + process.StartInfo.LoadUserProfile + "\n";
                processStartInfoAsString += "\tPassword: " + process.StartInfo.Password + "\n";
                processStartInfoAsString += "\tRedirect standard error: " + process.StartInfo.RedirectStandardError + "\n";
                processStartInfoAsString += "\tRedirect standard input: " + process.StartInfo.RedirectStandardInput + "\n";
                processStartInfoAsString += "\tRedirect standard output: " + process.StartInfo.RedirectStandardOutput + "\n";
                processStartInfoAsString += "\tStandard error encoding: " + process.StartInfo.StandardErrorEncoding + "\n";
                processStartInfoAsString += "\tStandard output encoding: " + process.StartInfo.StandardOutputEncoding + "\n";
                processStartInfoAsString += "\tUser name: " + process.StartInfo.UserName + "\n";
                processStartInfoAsString += "\tUse shell execute: " + process.StartInfo.UseShellExecute + "\n";
                processStartInfoAsString += "\tVerb: " + process.StartInfo.Verb + "\n";
                processStartInfoAsString += "\tVerbs: " + process.StartInfo.Verbs + "\n";
                processStartInfoAsString += "\tWindow style: " + process.StartInfo.WindowStyle + "\n";
                processStartInfoAsString += "\tWorking directory: " + process.StartInfo.WorkingDirectory + "\n";
            }

            return processStartInfoAsString;
        }

        public static string ProcessDetailsToString(
            Process process, bool includeThreadInfo = false, bool includeEnvironmentVariables = false)
        {
            string processDetailsAsString = "Process name: " + process.ProcessName + "\n";
            processDetailsAsString += "ID: " + process.Id + "\n";
            processDetailsAsString += "Base priority: " + process.BasePriority + "\n";
            processDetailsAsString += "Container: " + process.Container + "\n";
            processDetailsAsString += "Enable raising events: " + process.EnableRaisingEvents + "\n";

            try
            {
                if (process.HasExited)
                {
                    processDetailsAsString += "Has exited: " + process.HasExited + "\n";
                    processDetailsAsString += "Exit code: " + process.ExitCode + "\n";
                    processDetailsAsString += "Exit time: " + process.ExitTime + "\n";
                }
            }
            catch (Win32Exception)
            {
            }
            catch (InvalidOperationException)
            {
            }

            try
            {
                processDetailsAsString += "Handle: " + process.Handle + "\n";
            }
            catch (Win32Exception e)
            {
                processDetailsAsString += "Handle: " + e.Message + "\n";
            }
            catch (InvalidOperationException e)
            {
                processDetailsAsString += "Handle: " + e.Message + "\n";
            }

            processDetailsAsString += "Handle count: " + process.HandleCount + "\n";
            processDetailsAsString += "Machine name: " + process.MachineName + "\n";

            try
            {
                processDetailsAsString += "Main module: " + process.MainModule + "\n";

                processDetailsAsString += "\tBase address: " + process.MainModule.BaseAddress + "\n";
                processDetailsAsString += "\tContainer: " + process.MainModule.Container + "\n";
                processDetailsAsString += "\tEntry point address: " + process.MainModule.EntryPointAddress + "\n";
                processDetailsAsString += "\tFile name: " + process.MainModule.FileName + "\n";
                processDetailsAsString += "\tFile version info:\n" + process.MainModule.FileVersionInfo + "\n";
                processDetailsAsString += "\tModule memory size: " + process.MainModule.ModuleMemorySize + "\n";
                processDetailsAsString += "\tModule name: " + process.MainModule.ModuleName + "\n";
                processDetailsAsString += "\tSite: " + process.MainModule.Site + "\n";
            }
            catch (Win32Exception e)
            {
                processDetailsAsString += "Main module: " + e.Message + "\n";
            }
            catch (InvalidOperationException e)
            {
                processDetailsAsString += "Main module: " + e.Message + "\n";
            }

            processDetailsAsString += "Main window handle: " + process.MainWindowHandle + "\n";
            processDetailsAsString += "Main window title: " + process.MainWindowTitle + "\n";

            try
            {
                processDetailsAsString += "Max working set: " + process.MaxWorkingSet + "\n";
                processDetailsAsString += "Min working set: " + process.MinWorkingSet + "\n";
            }
            catch (Win32Exception e)
            {
                processDetailsAsString += "Max/min working set: " + e.Message + "\n";
            }
            catch (InvalidOperationException e)
            {
                processDetailsAsString += "Max/min working set: " + e.Message + "\n";
            }

            try
            {
                processDetailsAsString += "Priority boost enabled: " + process.PriorityBoostEnabled + "\n";
            }
            catch (Win32Exception e)
            {
                processDetailsAsString += "Priority boost enabled: " + e.Message + "\n";
            }
            catch (InvalidOperationException e)
            {
                processDetailsAsString += "Priority boost enabled: " + e.Message + "\n";
            }

            try
            {
                processDetailsAsString += "Priority class: " + process.PriorityClass + "\n";
            }
            catch (Win32Exception e)
            {
                processDetailsAsString += "Priority class: " + e.Message + "\n";
            }
            catch (InvalidOperationException e)
            {
                processDetailsAsString += "Priority class: " + e.Message + "\n";
            }

            try
            {
                processDetailsAsString += "Priviledged processor time: " + process.PrivilegedProcessorTime + "\n";
            }
            catch (Win32Exception e)
            {
                processDetailsAsString += "Priviledged processor time: " + e.Message + "\n";
            }
            catch (InvalidOperationException e)
            {
                processDetailsAsString += "Priviledged processor time: " + e.Message + "\n";
            }

            try
            {
                processDetailsAsString += "Processor affinity: " + process.ProcessorAffinity + "\n";
            }
            catch (Win32Exception e)
            {
                processDetailsAsString += "Processor affinity: " + e.Message + "\n";
            }
            catch (InvalidOperationException e)
            {
                processDetailsAsString += "Processor affinity: " + e.Message + "\n";
            }

            processDetailsAsString += "Responding: " + process.Responding + "\n";
            processDetailsAsString += "Session ID: " + process.SessionId + "\n";
            processDetailsAsString += "Site: " + process.Site + "\n";

            try
            {
                processDetailsAsString += "Standard error: " + process.StandardError + "\n";
            }
            catch (InvalidOperationException e)
            {
                processDetailsAsString += "Standard error: " + e.Message + "\n";
            }

            try
            {
                processDetailsAsString += "Standard input: " + process.StandardInput + "\n";
            }
            catch (InvalidOperationException e)
            {
                processDetailsAsString += "Standard input: " + e.Message + "\n";
            }

            try
            {
                processDetailsAsString += "Standard output: " + process.StandardOutput + "\n";
            }
            catch (InvalidOperationException e)
            {
                processDetailsAsString += "Standard output: " + e.Message + "\n";
            }

            processDetailsAsString += ProcessStartInfoToString(process, includeEnvironmentVariables);

            try
            {
                processDetailsAsString += "Start time: " + process.StartTime + "\n";
            }
            catch (Win32Exception e)
            {
                processDetailsAsString += "Start time: " + e.Message + "\n";
            }
            catch (InvalidOperationException e)
            {
                processDetailsAsString += "Start time: " + e.Message + "\n";
            }

            processDetailsAsString += "Synchronizing object: " + process.SynchronizingObject + "\n";
            processDetailsAsString += "Threads: (Thread count is " + process.Threads.Count + ")\n";

            if (includeThreadInfo && process.Threads != null)
            {
                for (int i = 0; i < process.Threads.Count; ++i)
                {
                    processDetailsAsString += "\tID: " + process.Threads[i].Id + "\n";
                    processDetailsAsString += "\t\tBase priority: " + process.Threads[i].BasePriority + "\n";
                    processDetailsAsString += "\t\tContainer: " + process.Threads[i].Container + "\n";
                    processDetailsAsString += "\t\tCurrent priority: " + process.Threads[i].CurrentPriority + "\n";

                    try
                    {
                        processDetailsAsString += "\t\tPriority boost enabled: " + process.Threads[i].PriorityBoostEnabled + "\n";
                    }
                    catch (Win32Exception e)
                    {
                        processDetailsAsString += "\t\tPriority boost enabled: " + e.Message + "\n";
                    }
                    catch (InvalidOperationException e)
                    {
                        processDetailsAsString += "\t\tPriority boost enabled: " + e.Message + "\n";
                    }

                    try
                    {
                        processDetailsAsString += "\t\tPriority level: " + process.Threads[i].PriorityLevel + "\n";
                    }
                    catch (Win32Exception e)
                    {
                        processDetailsAsString += "\t\tPriority level: " + e.Message + "\n";
                    }
                    catch (InvalidOperationException e)
                    {
                        processDetailsAsString += "\t\tPriority boost enabled: " + e.Message + "\n";
                    }

                    try
                    {
                        processDetailsAsString += "\t\tPriviledged processor time: " + process.Threads[i].PrivilegedProcessorTime + "\n";
                    }
                    catch (Win32Exception e)
                    {
                        processDetailsAsString += "\t\tPriviledged processor time: " + e.Message + "\n";
                    }
                    catch (InvalidOperationException e)
                    {
                        processDetailsAsString += "\t\tPriority boost enabled: " + e.Message + "\n";
                    }

                    processDetailsAsString += "\t\tSite: " + process.Threads[i].Site + "\n";
                    processDetailsAsString += "\t\tStart address: " + process.Threads[i].StartAddress + "\n";

                    try
                    {
                        processDetailsAsString += "\t\tStart time: " + process.Threads[i].StartTime + "\n";
                    }
                    catch (Win32Exception e)
                    {
                        processDetailsAsString += "\t\tStart time: " + e.Message + "\n";
                    }
                    catch (InvalidOperationException e)
                    {
                        processDetailsAsString += "\t\tPriority boost enabled: " + e.Message + "\n";
                    }

                    processDetailsAsString += "\t\tThread state: " + process.Threads[i].ThreadState + "\n";

                    try
                    {
                        processDetailsAsString += "\t\tTotal processor time: " + process.Threads[i].TotalProcessorTime + "\n";
                    }
                    catch (Win32Exception e)
                    {
                        processDetailsAsString += "\t\tTotal processor time: " + e.Message + "\n";
                    }
                    catch (InvalidOperationException e)
                    {
                        processDetailsAsString += "\t\tPriority boost enabled: " + e.Message + "\n";
                    }

                    try
                    {
                        processDetailsAsString += "\t\tUser processor time: " + process.Threads[i].UserProcessorTime + "\n";
                    }
                    catch (Win32Exception e)
                    {
                        processDetailsAsString += "\t\tUser processor time: " + e.Message + "\n";
                    }
                    catch (InvalidOperationException e)
                    {
                        processDetailsAsString += "\t\tPriority boost enabled: " + e.Message + "\n";
                    }

                    try
                    {
                        processDetailsAsString += "\t\tWait reason: " + process.Threads[i].WaitReason + "\n";
                    }
                    catch (InvalidOperationException e)
                    {
                        processDetailsAsString += "\t\tWait reason: " + e.Message + "\n";
                    }
                }
            }

            try
            {
                processDetailsAsString += "Total processor time: " + process.TotalProcessorTime + "\n";
            }
            catch (Win32Exception e)
            {
                processDetailsAsString += "Total processor time: " + e.Message + "\n";
            }
            catch (InvalidOperationException e)
            {
                processDetailsAsString += "Total processor time: " + e.Message + "\n";
            }

            try
            {
                processDetailsAsString += "User processor time: " + process.UserProcessorTime + "\n";
            }
            catch (Win32Exception e)
            {
                processDetailsAsString += "User processor time: " + e.Message + "\n";
            }
            catch (InvalidOperationException e)
            {
                processDetailsAsString += "User processor time: " + e.Message + "\n";
            }

            processDetailsAsString += "Working set: " + process.WorkingSet64;

            return processDetailsAsString;
        }

        #endregion For debugging and logging
    }
}
