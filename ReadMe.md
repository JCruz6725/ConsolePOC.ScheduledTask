# ConsolePOC.ScheduledTask


## Overview

A simple framework/boiler plate code that is to be used to build with the windows task scheduler.


## How to use:

Instantiated a new `Runner` class and call `.Run()` while passing the entry point to your process.


### Example usage
Util.cs
```csharp
public static void SomeAction() => thread.sleep(2000)
```
Main.cs
```csharp
Runner runner = new Runner()
runner.Run( () => { SomeAction(); } )
```

## Extending Functionality of the Runner

Addtionality, we can extend the functionality and create a derive class to override the `CustomGracefulShutdown`, `CustomCancellation`, and `CustomExit` methods. These additional controls are tide to the apps lifecycles.

## Overview
The `Runner` is a manager for the task that are scheduled to run with the Windows Task Scheduler.

Initializes tha
`Runner()`

### Properties

- **`bool TaskComplete`**
  - **Get** whether or not the inserted action ran by the runner has completed succesfully.
  - On instantiation is **false**

- **`bool TaskInterrupted`**
  - **Get** whether or not the inserted action ran by the runner was interrupted.
  - On instantiation is **false**

- **`bool HasGracefullyExited`**
  - **Get** whether or not the inserted action ran by the runner has shut down in a graceful manor successfully.
  - On instantiation is **false**
	
### Methods

- **`Void Run(Action insertedAction, Action? finalAction = null)`**
    - **Description:** Safely executes the passed in action and logs the start of the execution.
    - **Parameter:** 
      - `Action insertedAction` - A passed in action to be executed and managed by the runner class.
      - `Action? finalAction = null ` - An optional Action that will execute in the ``final`` block of the main `try` `catch`.
- **`Virtual Void CustomGracefulShutdown()`**
    - **Description:** The graceful shutdown sequence acts as a safe guard to dispose, or log any resources that are need. The CustomGracefulShutdown when overriden prodives an additional mechinism to log, dipose, capture data, or start other actions. Runs at on task complete or when the task is interupted by a termination signal. Always runs. 
- **`Virtual Void CustomCancellation()`**
    - **Description:**  The cancellation method or sequence is call when the app detects the termination 
- **`Virtual Void CustomExit()`**
    - **Description:** The Exit sequence terminal to all process. The CustomGracefulShutdown when overriden prodives an additional mechinism to log, dipose, capture data, or start other actions. Always runs and isa last line defence for actions or items. Items here must be as atomic as posible and must stable. **Exersise caution** when overiding as volitile actions/item will have have dire consequnces to overall integrety. Move volile action to GracefulShutdown sequence.  Always runs.

- **`Void InvokeCancellation()`**
  - **Description:** Programmaticly invoke the cancellation sequence. Fire off termination signal to console app. Equivlent to `ctr+c` or `ctr+break`.


## Remarks

With in the windows task scheduler, there is no way to view whether or not the task actually ran correctly. Although the history tab appears to be a way to determine whether it did it is miss leading. What it actually records/displays is whether the task schedualler it self was able to run the app. I.e. it would fail if the task scheduler has insufficeint persmisions to the app or wasnt able to locate the app in the location specified when creating the task. This is futher confirm by looking at the history tab and looking at the source column which all say task scheduler and not the app that we manully registered.

This is where the windows event viewer comes into play. With the registration we can can see, once we add the app as a task and schedual a run, that the app is printing the event view. Depending how the app is written  we and futher applify or dilute the amount of logs we see.

Addtionally If the app where to crash, unhandle execption, the Common Language Runtime (CLR) creates an event, a default behavior. There is a mechanism in the CLR to provide this log in the event viewer even with logging configutrrations with in the app. this is severly limited to unhandle exceptiong though and even then we are un able to distigish trait with in the clr event to ditinguse the what app crash. the event describes a proble with the clr and only by reading th edescription of the event are able to determin what app crash. This isnt ideal as you cannot trigger any aditional events to the the App it self. is you were to try and schedual a task base on this crash it event it would fire off every time a app crash the clr even if it is unrealated.

### Addtional Remarks
If using the event viewer for loging prior to the installation the app must be registered, use the following command. 

	New-EventLog -LogName Application -Source "ConsolePOC.ScheduledTask"

Creates a record in the regitry. Required, will cause a error in the event viewer.

### To-Do
Runner class has depedency in the nlog for logging. Need to be rewriten with di to fix.
