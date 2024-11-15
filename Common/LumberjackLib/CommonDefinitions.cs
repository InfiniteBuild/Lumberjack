
namespace Lumberjack.Interface
{
    [Flags]
    public enum LogLevel
    {
        All = 0xFFFF,
        UserInfo = 1,
        Info = 2,
        Warning = 4,
        Error = 8
    }
}