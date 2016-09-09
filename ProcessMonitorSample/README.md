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

Process Monitor WPF app utilizes [EventBasedProcessMonitorWin32](https://github.com/tompaana/my-2-bits/blob/master/ProcessMonitorSample/SharedCode/ProcessMonitoring/EventBasedProcessMonitorWin32.cs)
and can detect new (created), modified and removed (deleted) processes. The app
also features a process manager, which can manipulate processes (kill, restart,
suspend and resume). The process management is implemented in - you guessed it -
[ProcessManager class](https://github.com/tompaana/my-2-bits/blob/master/ProcessMonitorSample/SharedCode/ProcessMonitoring/ProcessManager.cs).
To test it [ProcessManagerTester class](https://github.com/tompaana/my-2-bits/blob/master/ProcessMonitorSample/ProcessMonitorWpf/ProcessManagerTester.cs)
is provided. In the project the `ProcessManagerTester` is hard-coded to react
on creation of any app (process) whose name starts with `notepad`. It will try
to kill and restart it (given that the [StartInfo](https://msdn.microsoft.com/en-us/library/system.diagnostics.processstartinfo(v=vs.110).aspx)
contains the process' file name), which might not be the case), then suspend and
resume. See [MainWindow.cs](https://github.com/tompaana/my-2-bits/blob/master/ProcessMonitorSample/ProcessMonitorWpf/MainWindow.xaml.cs)
to see how `ProcessManagerTester` is used.

### Summary ###

| Class                                                                                                                                                                         | Compatibility | Features                                                                                                                                                                                                                                              |
| ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [PollingProcessMonitor](https://github.com/tompaana/my-2-bits/blob/master/ProcessMonitorSample/SharedCode/ProcessMonitoring/PollingProcessMonitor.cs)                         | Win32         | Detects new (created), removed (deleted) processes and changes in foreground.                                                                                                                                                                         |
| [EventBasedProcessMonitorUniversal](https://github.com/tompaana/my-2-bits/blob/master/ProcessMonitorSample/SharedCode/ProcessMonitoring/EventBasedProcessMonitorUniversal.cs) | Win32, UWP    | Detects changes in foreground. Note: There are many other events that can be detected, but they did not apply to this solution. To learn more see [Event Constants](https://msdn.microsoft.com/en-us/library/windows/desktop/dd318066(v=vs.85).aspx). |
| [EventBasedProcessMonitorWin32](https://github.com/tompaana/my-2-bits/blob/master/ProcessMonitorSample/SharedCode/ProcessMonitoring/EventBasedProcessMonitorWin32.cs)         | Win32         | Detects new (created), modified and removed (deleted) processes.                                                                                                                                                                                      |
| [ProcessManager](https://github.com/tompaana/my-2-bits/blob/master/ProcessMonitorSample/SharedCode/ProcessMonitoring/ProcessManager.cs)                                       | Win32         | Can kill, restart, suspend and resume processes.                                                                                                                                                                                                      |

## AppService Bridge ##

AppService Bridge is a mechanism to allow inter-process communication between -
in this case - UWP and a standard Win32 console application. The message
exchange utilizes [ValueSet](https://msdn.microsoft.com/library/windows/apps/dn636131)
class. The message protocol used in this solution is JSON based. The UWP app is
in charge i.e. it sends the requests and waits for the console app to respond.

### Required changes in UWP app project and `App.xaml.cs` files ###

There are several changes that need to be done to UWP project files to enable
the AppService Bridge functionality. Most of the changes required are in
[Package.appxmanifest](https://github.com/tompaana/my-2-bits/blob/master/ProcessMonitorSample/ProcessMonitorUwp/Package.appxmanifest)
file.

1. You need to have restricted capabilities and desktop namespaces defined:

    ```xml
    <?xml version="1.0" encoding="utf-8"?>
    <Package
      xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10"
      xmlns:mp="http://schemas.microsoft.com/appx/2014/phone/manifest"
      xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10"
      xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities"
      xmlns:desktop="http://schemas.microsoft.com/appx/manifest/desktop/windows10"
      IgnorableNamespaces="uap mp rescap desktop">
      
      ...
    ```

2. Set `TargetDeviceFamily` `Name` to `Windows.Desktop` (Note that the version
   number may be different depending on your SDK and Windows version):

    ```xml
      <Dependencies>
        <!-- <TargetDeviceFamily Name="Windows.Universal" MinVersion="10.0.0.0" MaxVersionTested="10.0.0.0" /> -->
        <TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.14332.0" MaxVersionTested="10.0.14332.0" />
      </Dependencies>
    ```

3. Define the extension:

    ```xml
          ...
          
          </uap:VisualElements>
          <Extensions>
            <uap:Extension Category="windows.appService">
              <uap:AppService Name="CommunicationService" />
            </uap:Extension>
            <desktop:Extension Category="windows.fullTrustProcess" Executable="ProcessMonitorConsole.exe" />
          </Extensions>
        </Application>
        
        ...
    ```

4. To capabilities add `runFullTrust` (Note the namespace):

    ```xml
      ...
      
      </Applications>
      <Capabilities>
        <Capability Name="internetClient" />
        <rescap:Capability Name="runFullTrust" />
      </Capabilities>
    </Package>
    ```    
    
The console app executable is copied in [ProcessMonitorUwp.csproj](https://github.com/tompaana/my-2-bits/blob/master/ProcessMonitorSample/ProcessMonitorUwp/ProcessMonitorUwp.csproj)
as post-build event command:

```xml
  ...
  
  </PropertyGroup>
  <PropertyGroup>
    <PostBuildEvent>xcopy /y /s "$(SolutionDir)ProcessMonitorConsole\bin\$(Configuration)\ProcessMonitorConsole.exe" "$(SolutionDir)\ProcessMonitorUwp\bin\x64\$(Configuration)\AppX\"
xcopy /y /s "$(SolutionDir)ProcessMonitorConsole\bin\$(Configuration)\ProcessMonitorConsole.exe" "$(SolutionDir)\ProcessMonitorUwp\bin\x86\$(Configuration)\AppX\"</PostBuildEvent>
  </PropertyGroup>
  
  ...
  
</Project>
```

In [App.xaml.cs](https://github.com/tompaana/my-2-bits/blob/master/ProcessMonitorSample/ProcessMonitorUwp/App.xaml.cs)
you need to have `OnBackgroundActivated` implemented:

```cs
        public static AppServiceConnection AppServiceConnection
        {
            get;
            set;
        }

        private BackgroundTaskDeferral _appServiceDeferral;

        ...

        /// <summary>
        /// Initializes the app service on the host process 
        /// </summary>
        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);

            if (args.TaskInstance.TriggerDetails is AppServiceTriggerDetails)
            {
                _appServiceDeferral = args.TaskInstance.GetDeferral();
                AppServiceTriggerDetails details = args.TaskInstance.TriggerDetails as AppServiceTriggerDetails;
                AppServiceConnection = details.AppServiceConnection;
            }
        }
```

### See the implemenation and the dedicated sample app to learn more ###

* Console app
 * [AppServiceConnectionManager.cs](https://github.com/tompaana/my-2-bits/blob/master/ProcessMonitorSample/ProcessMonitorConsole/AppServiceConnectionManager.cs)
 * [AppServiceConnectionRequestHandler.cs](https://github.com/tompaana/my-2-bits/blob/master/ProcessMonitorSample/ProcessMonitorConsole/AppServiceConnectionRequestHandler.cs)
* UWP app
 * [AppServiceBridgeManager.cs](https://github.com/tompaana/my-2-bits/blob/master/ProcessMonitorSample/ProcessMonitorUwp/AppServiceBridgeManager.cs)
* Microsoft sample app: https://github.com/Microsoft/DesktopBridgeToUWP-Samples/tree/master/Samples/AppServiceBridgeSample

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
