
namespace Lumberjack.Interface
{
    public class LogChannelFile : ILoggingChannel, IDisposable
    {
        private string m_fileName;
        private StreamWriter m_writer;
        private int m_backupCount;
        private System.Timers.Timer m_timer;

        public LogDisplayFlags DisplayFlags { get; set; } = LogDisplayFlags.ShowTimestamp | LogDisplayFlags.ShowLevel;

        public LogLevel LevelFilter { get; set; } = LogLevel.All;

        public LogChannelFile(string fileName, int backupCount = 5)
        {
            m_backupCount = backupCount;
            m_fileName = fileName;

            CheckLogSwitchNeeded();

            if (m_writer == null)
            {
                FileStreamOptions options = new FileStreamOptions();
                options.Share = FileShare.ReadWrite;
                options.Mode = FileMode.Append;
                options.Access = FileAccess.Write;

                m_writer = new StreamWriter(m_fileName, options);
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

        public LogChannelFile(string fileName, LogLevel filter, LogDisplayFlags displayFlags, int backupCount = 5) : this(fileName, filter, backupCount)
        {
            DisplayFlags = displayFlags;
        }

        private void M_timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
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

                m_writer.WriteLine(outputText);

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

                FileStreamOptions options = new FileStreamOptions();
                options.Share = FileShare.ReadWrite;
                options.Mode = FileMode.CreateNew;
                options.Access = FileAccess.Write;

                m_writer = new StreamWriter(m_fileName, options);
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
