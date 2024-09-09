
namespace ConsolePOC.ScheduledTask {
    internal class Program {
        static async Task Main(string[] args) {
            Runner runner = new Runner();
            runner.Run(() => { runner.DoSomething(5).Wait(); throw new Exception("some error"); });
        }
       
    }
}
