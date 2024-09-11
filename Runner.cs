using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace ConsolePOC.ScheduledTask {

    /// <summary>
    /// A base class for safely executing and managing the state of a windows task. Hooks into the the console app lifecycle (Cancelation, Exit).
    /// </summary>
    class Runner {
        public bool TaskComplete { get; private set; } = false;
        public bool TaskInterrupt { get; private set; } = false;
        public bool HasGracefullyShutdown { get; private set; } = false;
        
        private ILogger<Program> _logger = LoggerFactory.Create(builder => builder.AddNLog()).CreateLogger<Program>();

        /// <summary>
        /// A base class for safely executing and managing the state of a windows task/operation within the windows task scheduler.
        /// </summary>
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
        /// <summary>
        /// Safely executes the <paramref name="insertedAction"/> and [<paramref name="finalAction"/>] within the framework of the the class.  
        /// </summary>
        /// <remarks>[<paramref name="finalAction"/>] is optional if included will run after <paramref name="insertedAction"/> but prior to cancellation, gracefulExit, and exit life cycle. Runs regardless if <paramref name="insertedAction"/> is successful.</remarks>
        /// <param name="insertedAction"></param>
        /// <param name="finalAction"></param>
        public void Run(Action insertedAction, Action? finalAction = null) {
            try {
                _logger.LogInformation("Task starting.");

                insertedAction();

                _logger.LogInformation("Task complete.");
                TaskComplete = true;
            }

            catch(Exception ex) {
                _logger.LogCritical(ex, $"Critical Error executing {nameof(insertedAction)}");
                TaskComplete = false;
            }
            finally {
                if (finalAction is not null)
                    finalAction();
            }
        }


        //Public API override interface. these functions to add addition actions. 

        /// <summary>
        /// Virtual method. Use to add additional functionality in the cancellation lifecycle.
        /// </summary>
        /// <remarks>A default behavior of doing nothing.</remarks>
        public virtual void CustomCancellation() { }


        /// <summary>
        /// Virtual method. Use to add additional functionality in the exit lifecycle.
        /// </summary>
        /// <remarks>A default behavior of doing nothing.</remarks>
        public virtual void CustomExit() { }


        /// <summary>
        /// Virtual method. Use to add additional functionality in the graceful shutdown lifecycle.
        /// </summary>
        /// <remarks>A default behavior of doing nothing.</remarks>
        public virtual void CustomGracefulShutdown() { }


        /// <summary>
        /// Manually Invoke the Cancellation lifecycle.
        /// </summary>
        /// <remarks> Will call CustomCancellation lifecycle.</remarks>
        public void InvokeCancellation() => _BaseCancel();
        #endregion


        #region private callers 
        // Private caller method to the Public API.
        // Used to Encapsulate additional logic and rules
        // while still allowing user to override


        private bool _CallerToCustomGracefulShutdown() {
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


        private void _CallerToCustomCancellation() {
            try {
                CustomCancellation();
            }
            catch(Exception ex) {
                _logger.LogError($"Error in the {nameof(CustomCancellation)}\n {ex.Message}\n {ex.StackTrace}");
            }
        }


        private void _CallerToCustomExit() {
            try {
                CustomExit();
            }
            catch(Exception ex) {
                _logger.LogError($"Error in the {nameof(CustomExit)}\n {ex.Message}\n {ex.StackTrace}");
                return;
            }
        }


        #region base


        private void _BaseCancel() {
            TaskInterrupt = true;
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


        private void Cancel(object? sender, ConsoleCancelEventArgs e) => _BaseCancel();


        private void Exit(object? sender, EventArgs e) {
            if(!HasGracefullyShutdown)
                HasGracefullyShutdown = _CallerToCustomGracefulShutdown();
            
            if(!TaskComplete || TaskInterrupt) {
                if(TaskInterrupt) {
                    _logger.LogCritical($"{nameof(Exit)} Invoked: Interrupted run.");
                    return;
                }
                if(!TaskComplete) {
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
