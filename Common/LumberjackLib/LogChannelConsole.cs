namespace Lumberjack.Interface
{
    public class LogChannelConsole : ILoggingChannel
    {
        public LogDisplayFlags DisplayFlags { get; set; } = LogDisplayFlags.ShowTimestamp | LogDisplayFlags.ShowLevel;

        public LogLevel LevelFilter { get; set; } = LogLevel.UserInfo;

        public string ComponentFilter { get; set; } = string.Empty;

        public LogChannelConsole()
        {
        }

        public LogChannelConsole(LogLevel filter) : this()
        {
            LevelFilter = filter;
        }

        public LogChannelConsole(LogLevel filter, LogDisplayFlags displayFlags) : this(filter)
        {
            DisplayFlags = displayFlags;
        }

        public void Close()
        {
            
        }

        public void LogMessage(LogLevel level, string message)
        {
            LogMessage(level, message, string.Empty);
        }

        public void LogMessage(LogLevel level, string message, string component)
        {
            if (!string.IsNullOrWhiteSpace(ComponentFilter))
            {
                if (string.IsNullOrWhiteSpace(component))
                    return;

                if (!component.Contains(ComponentFilter, StringComparison.InvariantCultureIgnoreCase))
                    return;
            }

            if (LevelFilter.HasFlag(level))
            {
                string outputText = string.Empty;
                if (DisplayFlags.HasFlag(LogDisplayFlags.ShowTimestamp))
                {
                    outputText += "[" + DateTimeOffset.Now.LocalDateTime.ToLongTimeString() + "] ";
                }

                if (DisplayFlags.HasFlag(LogDisplayFlags.ShowLevel))
                {
                    outputText += level.ToString() + ": ";
                }
                else
                {
                    if (level == LogLevel.Error)
                        outputText += level.ToString() + ": ";
                }

                outputText += message;

                Console.WriteLine(outputText);
            }    
                
        }
    }
}
