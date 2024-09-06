
namespace ConsolePOC.ScheduledTask {
    internal class Program {
        static async Task Main(string[] args) {
            Runner r = new Runner();
            await r.Run();
        }
    }
}
