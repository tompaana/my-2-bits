Process Monitor Sample
======================

This sample demonstrates various ways of monitoring and manipulating (killing,
restarting, suspending and resuming) processes on Windows. The nature of this
project is something of a Frankenstein - it is a collection of several solutions
found online in addition to my own contributions. The project is essentially the
result of my study on interacting with processes with C#, and which I decided to
share as a collection of different solutions to a problem.

For the source and the acknowledgements of the other authors (read: the
developers whose code I stole) see the end of this file.

Compatibility: Win32 and Universal Windows Platform. Developed with Visual
Studio 2015.

## Building and running the sample ##

Open the solution file (`ProcessMonitorSample.sln`) with Visual Studio and
select **Build** -> **Build Solution** (or hit Ctrl-Shift-B).

The solution consists of three projects: **ProcessMonitorConsole**,
**ProcessMonitorUwp** and **ProcessMonitorWpf**. To run the desired
project right-click it in the **Solution Explorer** and select
**Set as StartUp Project**. Press **F5** to start the project.

## Implementation ##

Although the names of the three projects match, they are all different in terms
of functionality.

### Process Monitor Console ###

ProcessMonitorConsole project is a Win32 console project. Since the project
does not have an event loop, it utilizes a polling process monitoring
implemented in [PollingProcessMonitor](https://github.com/tompaana/my-2-bits/blob/master/ProcessMonitorSample/SharedCode/ProcessMonitoring/PollingProcessMonitor.cs).
The polling process monitor can detect created and deleted processes as well as
which one is on the foreground. See [Program.cs](https://github.com/tompaana/my-2-bits/blob/master/ProcessMonitorSample/ProcessMonitorConsole/Program.cs)
and [ProcessMonitorEventHandler.cs](https://github.com/tompaana/my-2-bits/blob/master/ProcessMonitorSample/ProcessMonitorConsole/ProcessMonitorEventHandler.cs)
for more information on how to use the process monitor class.

### Process Monitor - UWP ###

Process Monitor UWP, as the name states, is an Universal Windows Platform app.
The biggest shortcoming in terms of process management in UWP is the lack of
[System.Diagnostics.Process class](https://msdn.microsoft.com/en-us/library/system.diagnostics.process(v=vs.110).aspx).
However, you can use event hooks with [SetWinEventHook ](https://msdn.microsoft.com/en-us/library/windows/desktop/dd373640(v=vs.85).aspx)
method:

```cs
private const uint WINEVENT_OUTOFCONTEXT = 0;
private const uint EVENT_SYSTEM_FOREGROUND = 3;
private WinEventProc _listener;
private IntPtr _winHook;
        
...

_listener = new WinEventProc(EventCallback);
_winHook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _listener, 0, 0, WINEVENT_OUTOFCONTEXT);
```

The callback signature is as follows:

```cs
void EventCallback(
    IntPtr hWinEventHook, uint iEvent, IntPtr hWnd, int idObject, int idChild,
    int dwEventThread, int dwmsEventTime)
```

See the whole class: [EventBasedProcessMonitorUniversal](https://github.com/tompaana/my-2-bits/blob/master/ProcessMonitorSample/SharedCode/ProcessMonitoring/EventBasedProcessMonitorUniversal.cs)

So what you get back about the process is its window handle and that's about it.
Luckily, with AppService Bridge we can establish a communication with a Win32
console app - in this case with ProcessMonitorConsole. We can then send a
request to the console app and get the process details back by the window
handle. For more information see [AppServiceBridgeManager.cs](https://github.com/tompaana/my-2-bits/blob/master/ProcessMonitorSample/ProcessMonitorUwp/AppServiceBridgeManager.cs).

Note: The `ProcessMonitorConsole.exe` is copied manually in the build process.
If the console app doesn't launch automatically with ProcessMonitorUwp app, try
rebuilding the solution.

### Process Monitor - WPF ###

### Summary ###

| Class                                                                                                                                                                         | Compatibility | Features                                                                                                                                                                                                                                              |
| ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [PollingProcessMonitor](https://github.com/tompaana/my-2-bits/blob/master/ProcessMonitorSample/SharedCode/ProcessMonitoring/PollingProcessMonitor.cs)                         | Win32         | Detects new (created), removed (deleted) processes and changes in foreground.                                                                                                                                                                         |
| [EventBasedProcessMonitorUniversal](https://github.com/tompaana/my-2-bits/blob/master/ProcessMonitorSample/SharedCode/ProcessMonitoring/EventBasedProcessMonitorUniversal.cs) | Win32, UWP    | Detects changes in foreground. Note: There are many other events that can be detected, but they did not apply to this solution. To learn more see [Event Constants](https://msdn.microsoft.com/en-us/library/windows/desktop/dd318066(v=vs.85).aspx). |
| [EventBasedProcessMonitorWin32](https://github.com/tompaana/my-2-bits/blob/master/ProcessMonitorSample/SharedCode/ProcessMonitoring/EventBasedProcessMonitorWin32.cs)         | Win32         | Detects new (created), modified and removed (deleted) processes.                                                                                                                                                                                      |
| [ProcessManager](https://github.com/tompaana/my-2-bits/blob/master/ProcessMonitorSample/SharedCode/ProcessMonitoring/ProcessManager.cs)                                       | Win32         | Can kill, restart, suspend and resume processes.                                                                                                                                                                                                      |

## AppService Bridge ##

TBD

## Source notice and acknowledgements ##

* Code in class [EventBasedProcessMonitorUniversal](https://github.com/tompaana/my-2-bits/blob/master/ProcessMonitorSample/SharedCode/ProcessMonitoring/EventBasedProcessMonitorUniversal.cs)
  was adopted from stack overflow answer by [James](http://stackoverflow.com/users/82586/james).
  The original stack overflow discussion - as of writing this - can found at
  http://stackoverflow.com/questions/17222788/fire-event-when-user-changes-active-process
* Code in class [EventBasedProcessMonitorWin32](https://github.com/tompaana/my-2-bits/blob/master/ProcessMonitorSample/SharedCode/ProcessMonitoring/EventBasedProcessMonitorWin32.cs)
  was adopted from a post in Wes' Puzzling Blog: http://weblogs.asp.net/whaggard/438006
* Code in class [ProcessManager](https://github.com/tompaana/my-2-bits/blob/master/ProcessMonitorSample/SharedCode/ProcessMonitoring/ProcessManager.cs)
  was adopted from stack overflow answer by [Magnus Johansson](http://stackoverflow.com/users/3584/magnus).
  The original stack overflow discussion - as of writing this - can found at
  http://stackoverflow.com/questions/71257/suspend-process-in-c-sharp
* **AppService Bridge** implementation is adopted from official Microsoft sample
  project licensed under MIT. See https://github.com/Microsoft/DesktopBridgeToUWP-Samples/tree/master/Samples/AppServiceBridgeSample
  and https://github.com/Microsoft/DesktopBridgeToUWP-Samples/blob/master/LICENSE
