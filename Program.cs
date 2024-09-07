
namespace ConsolePOC.ScheduledTask {
    internal class Program {
        static async Task Main(string[] args) {
            Runner runner = new Runner();
            await runner.Run(async () => { await runner.DoSomething(5); });
        }
       
    }
}
