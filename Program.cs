
namespace ConsolePOC.ScheduledTask {
    internal class Program {
        static async Task Main(string[] args) {
            Runner runner = new Runner();
            runner.Run(() => { DoSomething(5, 25).Wait(); });
        }


        public static async Task<int> DoSomething(int iterations, int? failPercentage = null) {
            for(int iter = 1; iter <= iterations; iter++) {
                await Task.Delay(1000);
                Console.WriteLine(iter);
                if (failPercentage is not null)
                    randomFailure((int)failPercentage);
            }
            return 0;
        }


        public static void randomFailure(int failPercentage) {
            Random random = new Random();
            int fail = random.Next(0,100);
            if(fail < failPercentage)
                throw new Exception("Random Failure");
        }
    }
}
