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
        bool _HasGracefullyShutdown = false;

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
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Cancel);


            

            /* 
             * CLR call this Event/Function the the program exit.
             * We add a new event that gets called in addition to other processes on exit
             * 
             * Confirmed that the process exit is called when task scheduler interrupts the process.
             */
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(Exit);
        }


        #region public inteface
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
        public virtual void CustomCancellation() { }
        public virtual void CustomExit() { }
        public virtual void CustomGracefulShutdown() { }

        
        /// <summary>
        /// Manually Invoke Cancellation. Will call CustomCancellation if overrode.
        /// </summary>
        public void InvokeCancellation() => _BaseCancel();
        #endregion


        #region private callers 
        // Private caller method to the Public API.
        // Used to Encapsulate additional logic and rules
        // while still allowing user to override
        bool _CallerToCustomGracefulShutdown() {
            _logger.LogInformation("Task clean up.");

            try {
                CustomGracefulShutdown();
                return true;
            }
            catch(Exception ex) {
                _logger.LogWarning($"Warning {nameof(CustomGracefulShutdown)} Did not complete \nMessage: {ex.Message}\n{ex.StackTrace} ");

                return false;
            }
        }

        void _CallerToCustomCancellation() {
            try {
                CustomCancellation();
            }
            catch(Exception ex) {
                _logger.LogError($"Error in the {nameof(CustomCancellation)}\n {ex.Message}\n {ex.StackTrace}");
            }
        }

        void _CallerToCustomExit() {
            try {
                CustomExit();
            }
            catch(Exception ex) {
                _logger.LogError($"Error in the {nameof(CustomExit)}\n {ex.Message}\n {ex.StackTrace}");
                return;
            }
        }

        #region base
        void _BaseCancel() {
            _TaskInterrupt = true;
            _logger.LogWarning($"{nameof(Cancel)}, Invoked");
            _CallerToCustomCancellation();

            // Invoke the ExitEvent. Required, will cause a hang in CancellationEvent if not called.
            // Exit code is not read by the OS as far as I can tell.
            Environment.Exit(1);

        }
        #endregion
        #endregion


        #region Hooks
        /*
         * Hooks below
         * Leave the blank returns this code is part of the shutdown sequence 
         * any throw exception here,or any top level exception, is thrown to the CLR Environment
         * that this will pollute the 'EventView' logs. NO INTENTIONAl TOP-LEVEL EXCEPTIONS.
         * Let logs do their jobs.
         */

        void Cancel(object? sender, ConsoleCancelEventArgs e) => _BaseCancel();

        void Exit(object? sender, EventArgs e) {
            if(!_HasGracefullyShutdown)
                _HasGracefullyShutdown = _CallerToCustomGracefulShutdown();
            
            if(!_TaskComplete || _TaskInterrupt) {
                if(_TaskInterrupt) {
                    _logger.LogCritical($"{nameof(Exit)} Invoked: Interrupted run.");
                    return;
                }
                if(!_TaskComplete) {
                    EventId a = new EventId(1000);
                    _logger.LogCritical(a,$"{nameof(Exit)} Invoked: Incomplete run.");
                    return;
                }
            }
            _CallerToCustomExit();


            _logger.LogInformation($"{nameof(Exit)}, Invoked: Run Complete.");
            return;
        }
        #endregion
    }
}
