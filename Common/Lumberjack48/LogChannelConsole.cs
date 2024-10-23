using System;

namespace Lumberjack.Interface
{
    public class LogChannelConsole : ILoggingChannel
    {
        public LogLevel LevelFilter { get; set; } = LogLevel.UserInfo;

        public LogChannelConsole()
        {
        }

        public LogChannelConsole(LogLevel filter) : this()
        {
            LevelFilter = filter;
        }

        public void Close()
        {
            
        }

        public void LogMessage(LogLevel level, string message)
        {
            if (LevelFilter.HasFlag(level))
                Console.WriteLine("[" + DateTimeOffset.Now.LocalDateTime.ToLongTimeString() + "] " + level.ToString() + ": " + message);
        }
    }
}
