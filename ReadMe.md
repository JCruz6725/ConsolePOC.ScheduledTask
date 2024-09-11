#ConsolePOC.ScheduledTask

runner class has depency in the nlog for logging. Need to be rewrite with


## Task Based Console app Framework/Boiler plate code

A simple framework/boiler plate code that is to be used in with the windows task scheduler. For batch or processes the



## How to use:

instantiated a new `Runner` class

call the `Runner.Run( () => { action(); })` and pass in entry point to your process.



## extening functionality

To extend the functionality create a derive class overriding the `CustomGracefulShutdown`, `CustomCancellation`, and `CustomExit` methods. These additional controls are tide to the apps lifecycles.



Overview

`Runner`



`Run(Action insertedAction, Action? finalAction)`


`CustomGracefulShutdown`

`CustomCancellation`

`CustomExit`





`GracefulShutdownSequence()`
The gracefulshutdown sequence act as a safe gaured to dispose, or log any resources that are need. The grace full sut down

The GracefulShudow when override prodives a mechinism to log, dipose, and capture data  by defaul nlo

will always run error thrown here are.


`ExitSequence()`
Thi




`CancelationSequence()`
the cancellation method or sequence is call when the app detects the termination 





If using the event viewer for loging prior to the installation the app must be registered, use the following command. 

	`New-EventLog -LogName Application -Source "ConsolePOC.ScheduledTask"`

Creates a record in the regitry. Required, will cause a error in the event viewer.








things worth noting

With in the windows task scheduler, there is no way to view whether or not the task actually ran correctly. Although the history tab appears to be a way to determine whether it did it is miss leading. What it actually records/displays is whether the task schedualler it self was able to run the app. I.e. it would fail if the task scheduler has insufficeint persmisions to the app or wasnt able to locate the app in the location specified when creating the task. This is futher confirm by looking at the history tab and looking at the source column which all say task scheduler and not the app that we manully registered.

This is where the windows event viewer comes into play. With the registration we can can see, once we add the app as a task and schedual a run, that the app is printing the event view. Depending how the app is written  we and futher applify or dilute the amount of logs we see.



Addtionally If the app where to crash, unhandle execption, the Common Language Runtime (CLR) creates an event, a default behavior. There is a mechanism in the CLR to provide this log in the event viewer even with logging configutrrations with in the app. this is severly limited to unhandle exceptiong though and even then we are un able to distigish trait with in the clr event to ditinguse the what app crash. the event describes a proble with the clr and only by reading th edescription of the event are able to determin what app crash. This isnt ideal as you cannot trigger any aditional events to the the App it self. is you were to try and schedual a task base on this crash it event it would fire off every time a app crash the clr even if it is unrealated. 





