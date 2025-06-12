
namespace Lumberjack.Interface
{
    public class LogEntry
    {
        public LogLevel Level { get; set; } = LogLevel.Info;
        public string Message { get; set; } = string.Empty;
        public string Component { get; set; } = string.Empty;

        public LogEntry(LogLevel level, string message, string component = "")
        {
            Level = level;
            Message = message.Trim();
            Component = component.Trim();
        }
    }
}
