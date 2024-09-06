
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace ConsolePOC.ScheduledTask {
    internal class Program {
        string State = "init";
        //bool Complete = false;
        bool CleanUpExecuted = false;
       
        static ILogger<Program> _loggerCreate() => LoggerFactory.Create(builder => builder.AddNLog()).CreateLogger<Program>();

        static async Task Main(string[] args) {
            var _logger = _loggerCreate(); 
            /*
             * Hypothesis: When the task scheduler and service managers call the 'stop' it trigger this ctr-c
             * this the the general termination signal hook in it with the new event.
             * 
             * When invoking the interupt as the console window is open it will invoke the cancelation event
             * 
             * Will not invoke when task in running and the task is 'end'-ed 
             * 
             */
            Console.CancelKeyPress  += new ConsoleCancelEventHandler(CancelationEvent);

            /* 
             * CLR call this Event/Function the the program exit.
             * We add a new event that gets called in addtion to other processes on exit
             * 
             * Confirmed that the process exit is called when task scheduler interups the process.
             */
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ExitEvent);
            
            try {
                // Get the request from task scheduler to gracefully exit the task 
                _logger.LogInformation("Task starting.");
                int _ = await ScheduledTaskRunner(10);    
                
                _logger.LogInformation("Task complete.");
			}

			catch(Exception) {
                await Task.Delay(1000);
                _logger.LogError("Error");
			}

            finally {
                await Task.Delay(1000);
                CleanDisposal();
            }

            await Task.Delay(1000);
            _logger.LogInformation("Task End.");
        }

        
        static async Task<int> ScheduledTaskRunner(int seconds) {
            var _logger = _loggerCreate();
            for(int i = 1; i <= seconds; i++) {
                await Task.Delay(1000); 
                _logger.LogInformation(i.ToString());
            }
            return 0;         
        }



        static void CleanDisposal() {
            var _logger = _loggerCreate();
            _logger.LogInformation("Task clean up.");
        }


        static void CancelationEvent(object? sender, ConsoleCancelEventArgs e ) {
            var _logger = _loggerCreate();
            
            _logger.LogWarning($"{nameof(CancelationEvent)}, Invoked");
            CleanDisposal();

            // Invoke the ExitEvent. Required, will cause a hang in CancellationEvent if not called
            Environment.Exit(0); 
        }


        static void ExitEvent(object? sender, EventArgs e) {
            var _logger = _loggerCreate();
            //if (!Complete)


            _logger.LogInformation($"{nameof(ExitEvent)}, Invoked");
        }
    }
}
