
using System.Collections.Concurrent;

namespace Lumberjack.Interface
{
    public class LogChannelFile : ILoggingChannel, IDisposable
    {
        private string m_fileName;
        private StreamWriter? m_writer;
        private int m_backupCount = 5;

        private ConcurrentQueue<LogEntry> m_logQueue = new ConcurrentQueue<LogEntry>();
        private Thread? m_logThread;
        private ManualResetEvent m_stopEvent = new ManualResetEvent(false);

        public LogDisplayFlags DisplayFlags { get; set; } = LogDisplayFlags.ShowTimestamp | LogDisplayFlags.ShowLevel;

        public LogLevel LevelFilter { get; set; } = LogLevel.All;

        public string ComponentFilter { get; set; } = string.Empty;

        public LogChannelFile(string fileName, bool reset, int backupCount = 5)
        {
            m_backupCount = backupCount;
            m_fileName = fileName;

            if (!reset)
                CheckLogSwitchNeeded();

            Open(reset);
        }

        public LogChannelFile(string fileName, bool reset, LogLevel filter, int backupCount = 5) : this(fileName, reset, backupCount)
        {
            LevelFilter = filter;
        }

        public LogChannelFile(string fileName, bool reset, LogLevel filter, LogDisplayFlags displayFlags, int backupCount = 5) : this(fileName, reset, filter, backupCount)
        {
            DisplayFlags = displayFlags;
        }

        public void LogMessage(LogLevel level, string message)
        {
            LogMessage(level, message, string.Empty);
        }

        public void LogMessage(LogLevel level, string message, string component)
        {
            LogEntry logEntry = new LogEntry(level, message, component);
            m_logQueue.Enqueue(logEntry);
        }

        private void LogProcessor()
        {
            bool entryWritten = false;

            while (m_stopEvent.WaitOne(0) == false)
            {
                if (m_logQueue.Count > 0)
                {
                    LogEntry? logEntry = null;
                    if (m_logQueue.TryDequeue(out logEntry))
                    {
                        if (logEntry == null)
                            continue;

                        CheckLogSwitchNeeded();

                        if (!string.IsNullOrWhiteSpace(ComponentFilter))
                        {
                            if (string.IsNullOrWhiteSpace(logEntry.Component))
                                return;

                            if (!logEntry.Component.Contains(ComponentFilter, StringComparison.InvariantCultureIgnoreCase))
                                return;
                        }

                        if (LevelFilter.HasFlag(logEntry.Level))
                        {
                            string outputText = string.Empty;
                            if (DisplayFlags.HasFlag(LogDisplayFlags.ShowTimestamp))
                            {
                                outputText += "[" + DateTimeOffset.Now.LocalDateTime.ToLongTimeString() + "] ";
                            }

                            if (DisplayFlags.HasFlag(LogDisplayFlags.ShowLevel))
                            {
                                outputText += logEntry.Level.ToString() + ": ";
                            }
                            else
                            {
                                if (logEntry.Level == LogLevel.Error)
                                    outputText += logEntry.Level.ToString() + ": ";
                            }

                            outputText += logEntry.Message;

                            m_writer.WriteLine(outputText);

                            entryWritten = true;
                        }
                    }
                }
                else
                {
                    Thread.Sleep(100); // Sleep for a short time to avoid busy waiting
                }

                if ((m_logQueue.Count == 0) && (entryWritten))
                {
                    entryWritten = false;
                    m_writer.Flush();
                }
            }
        }

        public void Close()
        {
            m_stopEvent.Set();

            if (m_logThread != null)
            {
                m_logThread.Join();
                m_logThread = null;
            }

            if (m_writer != null)
            {
                m_writer.Flush();
                m_writer.Close();
                m_writer = null;
            }
        }

        public void Open(bool resetLog = false)
        {
            if ((m_logThread != null) || (m_writer != null))
                Close();

            if (resetLog)
                BackupFiles();

            if (m_writer == null)
            {
                FileStreamOptions options = new FileStreamOptions();
                options.Share = FileShare.ReadWrite;
                options.Mode = FileMode.Append;
                options.Access = FileAccess.Write;

                m_writer = new StreamWriter(m_fileName, options);
                File.SetCreationTime(m_fileName, DateTimeOffset.Now.LocalDateTime);
            }

            m_stopEvent.Reset();
            m_logThread = new Thread(LogProcessor);
            m_logThread.Name = "LogChannelFileProcessor";
            m_logThread.IsBackground = true;
            m_logThread.Start();
        }

        protected void CheckLogSwitchNeeded()
        {
            if (File.Exists(m_fileName))
            {
                DateTime creation = File.GetCreationTime(m_fileName);
                if (creation.DayOfYear != DateTimeOffset.Now.DayOfYear)
                {
                    Close();
                    BackupFiles();
                    Open();
                }
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
