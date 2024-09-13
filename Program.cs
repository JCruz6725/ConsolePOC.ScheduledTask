using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;

namespace ConsolePOC.ScheduledTask {
    internal class Program {
        static void Main(string[] args) {
            var logging = LogManager.GetCurrentClassLogger();
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services => {
                    services.AddTransient<Runner>();
                    services.AddLogging((log) => {
                        log.ClearProviders();    
                        log.AddNLog(); 
                    });
                })
                .Build();
            Runner runner = host.Services.GetRequiredService<Runner>();
            runner.Run(() => { DoSomething(5).Wait(); });
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
