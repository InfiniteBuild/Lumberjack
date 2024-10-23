namespace LoggingLibInterface
{
    public enum LogDisplayFlags
    {
        None = 0,
        ShowTimestamp = 1,
        ShowLevel = 2,
    }

    public interface ILoggingChannel
    {
        LogDisplayFlags DisplayFlags { get; set; }
        LogLevel LevelFilter { get; set; }
        void LogMessage(LogLevel level, string message);
        void Close();
    }
}
