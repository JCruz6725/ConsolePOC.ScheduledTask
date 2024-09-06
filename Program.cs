
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace ConsolePOC.ScheduledTask {
    internal class Program {
        string State = "init";
        static ILogger<Program> _loggerCreate() => LoggerFactory.Create(builder => builder.AddNLog()).CreateLogger<Program>();

        static async Task Main(string[] args) {
            var _logger = _loggerCreate(); 
            /*
             * When the task scheduler and service managers call the 'stop' it trigger this ctr-c
             * this the the general termination signal hook in it with the new event.
             * 
             */
            Console.CancelKeyPress  += new ConsoleCancelEventHandler(CancelationEvent);

            /* 
             * CLR call this Event/Function the the program exit.
             * We add a new event that gets called in addtion to other processes on exit
             */
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ExitEvent);
            
            try {
                // Get the request from task scheduler to gracefully exit the task 
                _logger.LogInformation("Task starting.");
                int _ = await ScheduledTaskRunner(5);      
                _logger.LogInformation("Task complete.");
			}

			catch(Exception) {
                await Task.Delay(1000);
                Console.Error.WriteLine("Error");
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
            _logger.LogInformation($"{nameof(CancelationEvent)}, Invoked");
            CleanDisposal();
            Environment.Exit(0);
        }


        static void ExitEvent(object? sender, EventArgs e) {
            var _logger = _loggerCreate();
            _logger.LogInformation($"{nameof(ExitEvent)}, Invoked");
        }
    }
}
