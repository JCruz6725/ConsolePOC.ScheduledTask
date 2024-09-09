using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace ConsolePOC.ScheduledTask {
    /// <summary>
    /// Base Runner class usable
    /// </summary>
    class Runner {

        bool _TaskComplete = false;
        bool _TaskInterupt = false;
        bool _GracefulShutdown = false;

        ILogger<Program> _logger = LoggerFactory.Create(builder => builder.AddNLog()).CreateLogger<Program>();

        public Runner() {
           /*
            * Hypothesis: When the task scheduler and service managers call the 'stop' it trigger this ctr-c
            * this the the general termination signal hook in it with the new event. 
            * 
            * Hypothesis - wrong. Task Scheduer does not call this termination signal. It may not have one at all
            * 
            * When invoking the interupt as the console window is open it will invoke the cancelation event
            * 
            * Will not invoke when task in running and the task is 'end'-ed 
            */
            Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelationEvent);

           /* 
            * CLR call this Event/Function the the program exit.
            * We add a new event that gets called in addtion to other processes on exit
            * 
            * Confirmed that the process exit is called when task scheduler interups the process.
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
                _logger.LogCritical(ex, $"CriticalError executing {nameof(insertedAction)}");
                _TaskComplete = false;
            }

            finally {
                if (finalAction is not null)
                    finalAction();
            }
        }

        public async Task<int> DoSomething(int seconds) {
            for(int i = 1; i <= seconds; i++) {
                await Task.Delay(1000);
                _logger.LogInformation(i.ToString());
            }
            return 0;
        }


        //Public API override interface. these fuctions to add addtion actions. 
        public virtual void CancelationSequece() { }
        public virtual void ExitSequece() { }
        public virtual void GrancefulShutdownSequence() { }



        // Private caller mothod to the Public API 
        // Used to Encapsulate addional loginc and rule
        bool _GrancefulShutdownSequence() {
            GrancefulShutdownSequence();
            _logger.LogInformation("Task clean up.");
            return true;
        }
         
        void _CancelationSequece() => CancelationSequece();
        void _ExitSequece() => ExitSequece();

        /*
         * Hooks below
         * Leave the blank returns this code is part of the shutdown sequece 
         * any throw exception here,or any top level exception, is thrown to the CLR Eviromnent
         * that this will polute the 'EventView' logs. NO INTENTIONAl TOP-LEVEL EXCEPTIONS.
         * Let logs do their jobs.
         */
        void CancelationEvent(object? sender, ConsoleCancelEventArgs e) {
            _logger.LogWarning($"{nameof(CancelationEvent)}, Invoked");
            _TaskInterupt = true;
            if (_GracefulShutdown)
                _GracefulShutdown = _GrancefulShutdownSequence();
            
            try {
                _CancelationSequece();
            }
            catch(Exception ex) {
                _logger.LogError($"Error in the {nameof(CancelationSequece)}\n {ex.Message}\n {ex.StackTrace}");
            }

            // Invoke the ExitEvent. Required, will cause a hang in CancellationEvent if not called.
            // Exit code is not read by the OS as far as I can tell.
            Environment.Exit(1); 
        }


        void ExitEvent(object? sender, EventArgs e) {
            if(!_GracefulShutdown) 
                _GracefulShutdown = _GrancefulShutdownSequence();
            
            if(!_TaskComplete || _TaskInterupt) {
                if(!_TaskComplete) {
                    _logger.LogCritical($"{nameof(ExitEvent)} Invoked: Incomplete run.");
                    return;
                }
                if(_TaskInterupt) {
                    _logger.LogCritical($"{nameof(ExitEvent)} Invoked: Interupted run.");
                    return;
                }
            }
            try {
                _ExitSequece();
            }
            catch(Exception ex) {
                _logger.LogError($"Error in the {nameof(ExitSequece)}\n {ex.Message}\n {ex.StackTrace}");
            }

            _logger.LogInformation($"{nameof(ExitEvent)}, Invoked: Run Complete.");
            return;
        }
    }
}
