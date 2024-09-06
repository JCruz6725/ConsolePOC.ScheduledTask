
using System.Runtime.CompilerServices;

namespace ConsolePOC.ScheduledTask {
    internal class Program {
        string State = "init"; 
            
        static async Task Main(string[] args) {
                
            
            /*
             * When the task scheduler and service managers call the 'stop' it trigger this ctr-c
             * this the the general termination signal hook in it with the new event.
             * 
             */
                
            Console.CancelKeyPress  += new ConsoleCancelEventHandler(CancelationEvent);


            /* CLR call this Event/Function the the program exit.
             * We add a new event that gets called in addtion to other processes on exit
             */

            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ExitEvent);
			
            
            try {
                // Get the request from task scheduler to gracefully exit the task 
                Console.WriteLine("Task starting.");
                int _ = await ScheduledTaskRunner(5);      
                Console.WriteLine("Task complete.");
			}

			catch(Exception) {
                await Task.Delay(1000);
                Console.Error.WriteLine("Error");
			}

            finally {
                await Task.Delay(1000);
                Console.WriteLine("Task clean up."); 
            }

            await Task.Delay(1000);
            Console.WriteLine("Task End.");
        }

        
        static async Task<int> ScheduledTaskRunner(int seconds) {
            for(int i = 1; i <= seconds; i++) {
                await Task.Delay(1000); 
                Console.WriteLine(i.ToString());
            }
            return 0;         
        }


        static void CancelationEvent(object? sender, ConsoleCancelEventArgs e ) {
            Console.WriteLine($"{nameof(CancelationEvent)}, Invoked");
            Environment.Exit(0);
        }


        static void ExitEvent(object? sender, EventArgs e) {
            Console.WriteLine($"{nameof(ExitEvent)}, Invoked");
        }


    }
}
