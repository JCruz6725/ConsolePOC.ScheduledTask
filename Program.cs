
namespace ConsolePOC.ScheduledTask {
    internal class Program {
        static async Task Main(string[] args) {
            Runner runner = new Runner();
            runner.Run(async () => { await runner.DoSomething(5); });
        }
       
    }
}
