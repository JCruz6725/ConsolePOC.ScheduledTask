using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace ConsolePOC.ScheduledTask {
    internal class Runner {
        
        bool _TaskComplete = false;
        bool _TaskInterupt = false;
        bool _GracefulShutdown = false;
        
        ILogger<Program> _logger = LoggerFactory.Create(builder => builder.AddNLog()).CreateLogger<Program>();


        public Runner() {
            /*
            * Hypothesis: When the task scheduler and service managers call the 'stop' it trigger this ctr-c
            * this the the general termination signal hook in it with the new event.
            * 
            * When invoking the interupt as the console window is open it will invoke the cancelation event
            * 
            * Will not invoke when task in running and the task is 'end'-ed 
            * 
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


        public async Task Run() {
            try {
                _logger.LogInformation("Task starting.");
                
                //Do something here
                int _ = await DoSomething(2);


                _logger.LogInformation("Task complete.");
                _TaskComplete = true;
            }

            catch(Exception ex) {
                await Task.Delay(1000);
                _logger.LogCritical(ex, "CritalError");

            }

            finally {
                await Task.Delay(1000);
                _GracefulShutdown =  GrancefulAppShutdown();
            }

        }

        
        async Task<int> DoSomething(int seconds) {
            for(int i = 1; i <= seconds; i++) {
                await Task.Delay(1000);
                _logger.LogInformation(i.ToString());
            }
            return 0;
        }

        



        bool GrancefulAppShutdown() {

            //Add AnyShut down Logic here...

            _logger.LogInformation("Task clean up.");
            return true;
        }


         

        /*
         * Hooks below
         * Leave the blank returns this code is part of the shutdown sequece 
         * any throw exception here,or any top level exception, is thrown to the CLR Eviromnent
         * that this will polute the 'EventView' logs. NO INTENTIONAl TOP-LEVEL EXCEPTIONS.
         * Let logs do their jobs.
         */
        void CancelationEvent(object? sender, ConsoleCancelEventArgs e) {
            _logger.LogWarning($"{nameof(CancelationEvent)}, Invoked");
            GrancefulAppShutdown();
            _TaskInterupt = true;

            // Invoke the ExitEvent. Required, will cause a hang in CancellationEvent if not called
            Environment.Exit(0); 
        }


        void ExitEvent(object? sender, EventArgs e) {
   
            if (!_TaskComplete || _TaskInterupt ) {
                if(!_TaskComplete) { 
                    _logger.LogCritical($"{nameof(ExitEvent)} Invoked: Incomplete run.");
                    return;
                }
                if(!_TaskInterupt) {
                    _logger.LogCritical($"{nameof(ExitEvent)} Invoked: Interupted run.");
                    return;
                }
            }
            
            _logger.LogInformation($"{nameof(ExitEvent)}, Invoked: Run Complete.");
            return;
        }
    }
}
