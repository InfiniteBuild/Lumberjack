namespace Lumberjack.Interface
{
    public class Logging
    {
        private static Logging m_instance;

        public static Logging Instance 
        {
            get 
            {
                if (m_instance == null) 
                    m_instance = new Logging();

                return m_instance;
            }
        }

        private List<ILoggingChannel> m_loggingChannels = new List<ILoggingChannel>();

        protected Logging()
        {

        }

        public void RegisterChannel(ILoggingChannel channel)
        {
            m_loggingChannels.Add(channel);
        }

        public void UnregisterChannel(ILoggingChannel channel)
        {
            if (m_loggingChannels.Contains(channel))
            {
                channel.Close();
                m_loggingChannels.Remove(channel);
            }
        }

        public void LogMessage(LogLevel level, string message)
        {
            foreach (ILoggingChannel channel in m_loggingChannels)
            {
                channel.LogMessage(level, message);
            }
        }

        public void CloseAllLogs()
        {
            foreach (ILoggingChannel channel in m_loggingChannels)
            {
                channel.Close();
            }

            m_loggingChannels.Clear();
        }

        public static void LogUserMessage(string message)
        {
            Instance.LogMessage(LogLevel.UserInfo, message);
        }

        public static void LogInfo(string message)
        {
            Instance.LogMessage(LogLevel.Info, message);
        }

        public static void LogWarning(string message)
        {
            Instance.LogMessage(LogLevel.Warning, message);
        }

        public static void LogError(string message)
        {
            Instance.LogMessage(LogLevel.Error, message);
        }

        public static void Close()
        {
            Instance.CloseAllLogs();

        }

        public static void CreateLogFile(string filepath, LogLevel filter = LogLevel.All)
        {
            Instance.RegisterChannel(new LogChannelFile(filepath, filter));
        }

        public static void CreateConsoleLog(LogLevel filter = LogLevel.UserInfo)
        {
            Instance.RegisterChannel(new LogChannelConsole(filter));
        }
    }
}
