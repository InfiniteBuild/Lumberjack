using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoggingLibInterface
{
    public class Feedback
    {
        private static Feedback m_instance;

        public static Feedback Instance 
        {
            get 
            {
                if (m_instance == null) 
                    m_instance = new Feedback();

                return m_instance;
            }
        }

        private List<ILoggingChannel> m_loggingChannels = new List<ILoggingChannel>();

        protected Feedback()
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

        public static void CreateLogFile(string filepath)
        {
            Instance.RegisterChannel(new LogChannelFile(filepath));
        }

        public static void CreateConsoleLog()
        {
            Instance.RegisterChannel(new LogChannelConsole());
        }
    }
}
