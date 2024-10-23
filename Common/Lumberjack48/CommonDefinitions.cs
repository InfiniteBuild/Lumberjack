
using System;

namespace LoggingLibInterface
{
    [Flags]
    public enum LogLevel
    {
        UserInfo,
        Info,
        Warning,
        Error
    }
}