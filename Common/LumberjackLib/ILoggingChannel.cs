namespace Lumberjack.Interface
{
    public interface ILoggingChannel
    {
        LogDisplayFlags DisplayFlags { get; set; }
        LogLevel LevelFilter { get; set; }
        string ComponentFilter { get; set; }
        void LogMessage(LogLevel level, string message);
        void LogMessage(LogLevel level, string message, string component);
        void Close();
        void Open(bool resetLog = false);
    }
}
