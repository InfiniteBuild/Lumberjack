namespace Lumberjack.Interface
{
    public interface ILoggingChannel
    {
        LogLevel LevelFilter { get; set; }
        void LogMessage(LogLevel level, string message);
        void Close();
    }
}
