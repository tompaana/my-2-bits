Process Monitor Sample
======================

This sample demonstrates various ways of monitoring and manipulating (killing,
restarting, suspending and resuming) process on Windows. The nature of this
project is something of a Frankenstein - it is a collection of several solutions
found online in addition to my own contributions. The project is essentially the
result of my study on interacting with processes with C# and which I decided to
share as a sort of collection of different solutions to a problem.

For the source and the acknowledgements of the other authors (read: the
contributors whose code I stole) see the end of this file.

## Building and running the sample ##

TBD

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
  was adopted from stack overflow answer by [Magnus Johanson](http://stackoverflow.com/users/3584/magnus).
  The original stack overflow discussion - as of writing this - can found at
  http://stackoverflow.com/questions/71257/suspend-process-in-c-sharp
* **AppService Bridge** implementation is adopted from official Microsoft sample
  project licensed under MIT. See https://github.com/Microsoft/DesktopBridgeToUWP-Samples/tree/master/Samples/AppServiceBridgeSample
  and https://github.com/Microsoft/DesktopBridgeToUWP-Samples/blob/master/LICENSE
