namespace ConsolePOC.ScheduledTask {
    internal class Program {
        static void Main(string[] args) {

			try {
                Console.WriteLine("Starting task.");
                int _ = ScheduledTaskRunner(5);      
                Console.WriteLine("Task complete.");
			}

			catch(Exception) {
                Task.Delay(1000).Wait();
                Console.WriteLine("Error");
			}

            finally {
                Task.Delay(1000).Wait();
                Console.WriteLine("Task clean up."); 
            }

            Task.Delay(1000).Wait();
            Console.WriteLine("Task End.");
        }


        static int ScheduledTaskRunner(int seconds) {
            for(int i = 0; i < seconds; i++) { 
                Task.Delay(1000).Wait(); 
                Console.WriteLine(i.ToString());
            }
            return 0;         
        }
    }
}
