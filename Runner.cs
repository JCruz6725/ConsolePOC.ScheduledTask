using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace ConsolePOC.ScheduledTask {
    /// <summary>
    /// Base Runner class usable
    /// </summary>
    class Runner {

        // Move into a Task-Like Object.......
        bool _TaskComplete = false;
        bool _TaskInterrupt = false;
        bool _GracefulShutdown = false;

        ILogger<Program> _logger = LoggerFactory.Create(builder => builder.AddNLog()).CreateLogger<Program>();

        public Runner() {
           /*
            * Hypothesis: When the task scheduler and service managers call the 'stop' it trigger this ctr-c
            * this the the general termination signal hook in it with the new event. 
            * 
            * Hypothesis - wrong. Task Scheduler does not call this termination signal. It may not have one at all
            * 
            * When invoking the interrupt as the console window is open it will invoke the cancelation event
            * 
            * Will not invoke when task in running and the task is 'end'-ed 
            */
            Console.CancelKeyPress += new ConsoleCancelEventHandler(CancellationEvent);

           /* 
            * CLR call this Event/Function the the program exit.
            * We add a new event that gets called in addition to other processes on exit
            * 
            * Confirmed that the process exit is called when task scheduler interrupts the process.
            */
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ExitEvent);
        }


        public void Run(Action insertedAction, Action? finalAction = null) {
            try {
                _logger.LogInformation("Task starting.");

                insertedAction();

                _logger.LogInformation("Task complete.");
                _TaskComplete = true;
            }

            catch(Exception ex) {
                _logger.LogCritical(ex, $"Critical Error executing {nameof(insertedAction)}");
                _TaskComplete = false;
            }

            finally {
                if (finalAction is not null)
                    finalAction();
            }
        }


        //Public API override interface. these functions to add addition actions. 
        public virtual void CancelationSequence() { }
        public virtual void ExitSequence() { }
        public virtual void GracefulShutdownSequence() { }



        // Private caller method to the Public API 
        // Used to Encapsulate additional logic and rule
        bool _GracefulShutdownSequence() {
            GracefulShutdownSequence();
            _logger.LogInformation("Task clean up.");
            return true;
        }
         
        void _CancelationSequence() => CancelationSequence();
        void _ExitSequence() => ExitSequence();

        /*
         * Hooks below
         * Leave the blank returns this code is part of the shutdown sequence 
         * any throw exception here,or any top level exception, is thrown to the CLR Environment
         * that this will pollute the 'EventView' logs. NO INTENTIONAl TOP-LEVEL EXCEPTIONS.
         * Let logs do their jobs.
         */
        void CancellationEvent(object? sender, ConsoleCancelEventArgs e) {
            _logger.LogWarning($"{nameof(CancellationEvent)}, Invoked");
            _TaskInterrupt = true;
            if (_GracefulShutdown)
                _GracefulShutdown = _GracefulShutdownSequence();
            
            try {
                _CancelationSequence();
            }
            catch(Exception ex) {
                _logger.LogError($"Error in the {nameof(CancelationSequence)}\n {ex.Message}\n {ex.StackTrace}");
            }

            // Invoke the ExitEvent. Required, will cause a hang in CancellationEvent if not called.
            // Exit code is not read by the OS as far as I can tell.
            Environment.Exit(1); 
        }


        void ExitEvent(object? sender, EventArgs e) {
            if(!_GracefulShutdown) 
                _GracefulShutdown = _GracefulShutdownSequence();
            
            // Convert to switch statement? only one can be active at a time
            if(!_TaskComplete || _TaskInterrupt) {
                if(_TaskInterrupt) {
                    _logger.LogCritical($"{nameof(ExitEvent)} Invoked: Interupted run.");
                    return;
                }
                if(!_TaskComplete) {
                    _logger.LogCritical($"{nameof(ExitEvent)} Invoked: Incomplete run.");
                    return;
                }
            }
            try {
                _ExitSequence();
            }
            catch(Exception ex) {
                _logger.LogError($"Error in the {nameof(ExitSequence)}\n {ex.Message}\n {ex.StackTrace}");
            }

            _logger.LogInformation($"{nameof(ExitEvent)}, Invoked: Run Complete.");
            return;
        }
    }
}
