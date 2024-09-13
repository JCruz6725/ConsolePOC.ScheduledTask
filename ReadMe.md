# ConsolePOC.ScheduledTask  
**Author:** John L. Cruz Jr.

## Overview

"ConsolePOC.ScheduledTask" is a proof-of-concept project showcasing the extendability of building a console app that the Windows Task Scheduler can execute. We can manage the task through the small framework/library by layering a manager on top of an incoming process to track states. 

### App lifecycle
```mermaid
graph TD;
 id([AppStart])-->MainLoop;
 MainLoop-->GracefulExit;
 MainLoop-->Cancellation;
 Cancellation-->GracefulExit;
 GracefulExit-->Exit;
 Exit-->id1([AppTermination]);
```


## How to use:

Instantiate a new `Runner` class and call `.Run(Action insertedAction);` while passing the entry point to your process. Instantiate and run on the topmost level of the console app.


### Example usage
Util.cs
```csharp
public static void SomeAction() => thread.sleep(2000)
```
Main.cs
```csharp
public void Main (string[] args){
    Runner runner = new Runner()
    runner.Run( () => { SomeAction(); } )
}
```

## Extending Functionality of the Runner

Additionally, we can extend the functionality and create a derived class to override the `CustomGracefulShutdown`, `CustomCancellation`, and `CustomExit` methods. These additional control overrides allow you to hook directly into their respective life cycle events.

## Overview
The `Runner` is a manager for tasks scheduled to run with the Windows Task Scheduler. It is a base class that contains basic functionality and can be extended.

### Constructor
- **`Runner()`**
  - **Description:** Initializes class.

### Properties
- **`bool TaskComplete`**
  - **Get** whether or not the inserted Action ran by the runner has been completed successfully.
  - On instantiation is **false**

- **`bool TaskInterrupted`**
  - **Get** whether or not the inserted Action ran by the runner was interrupted.
  - On instantiation is **false**

- **`bool HasGracefullyExited`**
  - **Get** whether or not the inserted Action ran by the runner has shut down in a graceful manner successfully.
  - On instantiation is **false**
  
### Methods

#### Concrete
- **`Void Run(Action insertedAction, Action? finalAction = null)`**
    - **Description:** Safely executes the passed-in Action and logs the state of the execution.
    - **Parameter:** 
      - `Action insertedAction` - A passed-in action to be executed and managed by the runner class.
      - `Action? finalAction = null ` - An optional Action that will execute in the ``final`` block of the main `try` `catch`.
- **`Void InvokeCancellation()`**
  - **Description:** Programmatically invoke the cancellation sequence. Fire off termination signal to console App. Equivalent to `ctr+c` or `ctr+break`.

#### Virtual
- **`Virtual Void CustomGracefulShutdown()`**
    - **Description:** Add additional custom logic to the graceful shutdown sequence. Act as a safeguard to dispose of or log any needed resources. Runs on task completion or when the task is interrupted by a termination signal. Always runs. 
- **`Virtual Void CustomCancellation()`**
    - **Description:** Add additional custom logic to the cancellation sequence. When the App receives an interruption signal, it will immediately be followed up with the cancellation sequence. 
- **`Virtual Void CustomExit()`**
    - **Description:** Add additional custom logic to the Exit sequence. The Exit sequence terminal to all app processes. It will always run and is a last-line defense for actions or items. Items here must be as atomic as possible and must be stable. **Exercise caution** when overriding as volatile actions/items will have dire consequences to overall app integrity. Move volatile Action to the GracefulShutdown sequence.


## Remarks

In the Windows Task Scheduler, there is no proper way to determine whether or not the task ran correctly. Although the App can return an exit code to the OS, the Task Scheduler does not have direct access. I suspect that when the App passes an exit code to the OS, the OS does not pass it to the Task Scheduler, or if it does, there isn't any provision to act upon it. This theory is further solidified by looking at the history tab; closer inspection of the source column reveals that only the task scheduler observes itself; what makes it into this history is unknown other than generic 'task starting, schedule, etc.'  sourcing itself. 

Additionally, if the App crashes, an unhandled exception, the Common Language Runtime (CLR) creates an event in the Windows Event Viewer, a default behavior. CLR-created events are severely limited to unhandled exceptions/crashes, though even then, we are unable to distinguish error traits within as they all fall under the CLR source. Only by looking at the description are you able to see what App crashed; this isn't ideal as you cannot trigger any additional events or tasks from the Task Scheduler from the App's failure. If the App does fail and is handled internally, the executions and process failures can be logged into whatever logging source. In other words, you have to make accommodations programmatically in the App to log the logs.

### Additional Remarks: Event Viewer
If you are using the event viewer for logging prior to the installation the App must be registered using the following command. 

 New-EventLog -LogName Application -Source "ConsolePOC.ScheduledTask"

Creates a record in the registry. Required; will cause an error in the event viewer.

With the registration and configuration to the event viewer, we can see that once we add the App as a task and schedule a run, the App transmits logs to the event view. Depending on how the App logging is configured, we can further amplify or dilute the number of logs we see.

If you need a mechanism to look at "exit codes," you can use the eventID as a pseudo mechanism. You would have an eventID tied to a correct source, which in this case would be the "ConsolePOC.ScheduledTask." In theory, that should work and be sufficient.

#### Source
 https://github.com/NLog/NLog/wiki/EventLog-target#notes

### To-Do
- [ ] Runner class has a dependency in the NLog for logging. Need to be rewritten with DI to fix it.

- [ ] Fix Constructor documentation.

- [ ] Rename the project to ..?..