
using System;
using System.IO;

namespace Lumberjack.Interface
{
    public class LogChannelFile : ILoggingChannel, IDisposable
    {
        private string m_fileName;
        private StreamWriter m_writer;
        private int m_backupCount;
        private System.Timers.Timer m_timer;

        public LogLevel LevelFilter { get; set; } = LogLevel.All;

        public LogChannelFile(string fileName, int backupCount = 5)
        {
            m_backupCount = backupCount;
            m_fileName = fileName;

            CheckLogSwitchNeeded();

            if (m_writer == null)
            {
                FileStream stream = new FileStream(m_fileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                m_writer = new StreamWriter(stream);
                File.SetCreationTime(m_fileName, DateTimeOffset.Now.LocalDateTime);
            }

            m_timer = new System.Timers.Timer(1000);
            m_timer.AutoReset = false;
            m_timer.Elapsed += M_timer_Elapsed;
        }

        public LogChannelFile(string fileName, LogLevel filter, int backupCount = 5) : this(fileName, backupCount)
        {
            LevelFilter = filter;
        }

        private void M_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            m_timer.Stop();
            if (m_writer != null)
                m_writer.Flush();
        }

        public void LogMessage(LogLevel level, string message)
        {
            CheckLogSwitchNeeded();

            if (LevelFilter.HasFlag(level))
            {
                m_writer.WriteLine("[" + DateTimeOffset.Now.LocalDateTime.ToLongTimeString() + "] " + level.ToString() + ": " + message);

                if (m_timer.Enabled)
                    m_timer.Stop();

                m_timer.Start();
            }
        }

        public void Close()
        {
            if ((m_timer != null) && (m_timer.Enabled))
                m_timer.Stop();

            if (m_writer != null)
            {
                m_writer.Flush();
                m_writer.Close();
            }
            m_writer = null;
        }

        protected void CheckLogSwitchNeeded()
        {
            DateTime creation = File.GetCreationTime(m_fileName);
            if (creation.DayOfYear != DateTimeOffset.Now.DayOfYear)
            {
                Close();
                BackupFiles();

                FileStream stream = new FileStream(m_fileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                m_writer = new StreamWriter(stream);
                File.SetCreationTime(m_fileName, DateTimeOffset.Now.LocalDateTime);
            }
        }

        protected void BackupFiles()
        {
            if (File.Exists(m_fileName + "." + m_backupCount))
            {
                File.Delete(m_fileName + "." + m_backupCount);
            }

            for(int i = m_backupCount - 1; i > 0; i--)
            {
                if (File.Exists(m_fileName + "." + i))
                {
                    File.Move(m_fileName + "." + i, m_fileName + "." + (i + 1));
                }
            }

            if (File.Exists(m_fileName))
            {
                File.Move(m_fileName, m_fileName + ".1");
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}
