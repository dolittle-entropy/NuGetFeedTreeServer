using System.Threading.Tasks;
using NuGet.Common;

namespace Server
{
    public class CustomLogger : LoggerBase
    {
        public override void Log(ILogMessage message)
        {
            System.Console.WriteLine($"[{message.Level}] {message.Message}");
        }

        public override Task LogAsync(ILogMessage message)
        {
            System.Console.WriteLine($"[{message.Level}] {message.Message}");
            return Task.CompletedTask;
        }
    }
}