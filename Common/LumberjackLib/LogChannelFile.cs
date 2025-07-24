
using System.Collections.Concurrent;
using System.Threading;

namespace Lumberjack.Interface
{
    public class LogChannelFile : ILoggingChannel, IDisposable
    {
        private string m_fileName;
        private StreamWriter? m_writer;
        private int m_backupCount = 5;

        private ConcurrentQueue<LogEntry> m_logQueue = new ConcurrentQueue<LogEntry>();
        private Task? m_processingTask = null;
        private SemaphoreSlim m_newItemSignal = new SemaphoreSlim(0);
        private CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource();

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
            m_newItemSignal.Release();
        }

        private async Task LogProcessor()
        {
            CancellationToken cancellationToken = m_cancellationTokenSource.Token;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await m_newItemSignal.WaitAsync(cancellationToken);

                    while (m_logQueue.TryDequeue(out LogEntry? logEntry))
                    {
                        CheckLogSwitchNeeded();

                        bool writeEntry = true;

                        if (!string.IsNullOrWhiteSpace(ComponentFilter))
                        {
                            if (string.IsNullOrWhiteSpace(logEntry.Component))
                                writeEntry = false;

                            if (!logEntry.Component.Contains(ComponentFilter, StringComparison.InvariantCultureIgnoreCase))
                                writeEntry = false;
                        }

                        if (!LevelFilter.HasFlag(LogLevel.All) && !LevelFilter.HasFlag(logEntry.Level))
                            writeEntry = false;

                        if (writeEntry)
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
                        }
                    }

                    m_writer.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An error occurred in the log processor: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("FileLog processor thread exiting.");
                if (m_writer != null)
                {
                    m_writer.Flush();
                    m_writer.Close();
                    m_writer = null;
                }
            }
        }

        public void Close()
        {
            if (m_processingTask == null)
            {
                Console.WriteLine("FileLog processor is not running, nothing to close.");
                return;
            }

            m_cancellationTokenSource.Cancel();
            m_newItemSignal.Release(); // Ensure we release the semaphore for any remaining items

            m_processingTask.Wait();
        }

        public void Open(bool resetLog = false)
        {
            if ((m_processingTask != null) && !m_processingTask.IsCompleted)
            {
                if (resetLog)
                {
                    Console.WriteLine("Rest Logfile - close logging");
                    Close();
                }
                else
                {
                    return;
                }
            }

            if (resetLog)
            {
                Console.WriteLine("Reset Logfile - backup old logfile and create new one");
                BackupFiles();
            }

            if (m_writer == null)
            {
                FileStreamOptions options = new FileStreamOptions();
                options.Share = FileShare.ReadWrite;
                options.Mode = FileMode.Append;
                options.Access = FileAccess.Write;

                m_writer = new StreamWriter(m_fileName, options);
                File.SetCreationTime(m_fileName, DateTimeOffset.Now.LocalDateTime);
            }

            Console.WriteLine("Starting FileLog processor...");

            m_cancellationTokenSource = new CancellationTokenSource();
            m_processingTask = Task.Factory.StartNew(
            ()=> LogProcessor(),
            m_cancellationTokenSource.Token,
            TaskCreationOptions.LongRunning, // Crucial hint for dedicated thread-like behavior
            TaskScheduler.Default).Unwrap();

            Console.WriteLine("FileLog processor started: " + m_fileName);
        }

        protected void CheckLogSwitchNeeded()
        {
            if (File.Exists(m_fileName))
            {
                DateTime creation = File.GetCreationTime(m_fileName);
                if (creation.DayOfYear != DateTimeOffset.Now.DayOfYear)
                {
                    BackupFiles();
                }
            }
        }

        protected void BackupFiles()
        {
            if (m_writer != null)
            {
                m_writer.Flush();
                m_writer.Close();
                m_writer = null;
            }

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

            FileStreamOptions options = new FileStreamOptions();
            options.Share = FileShare.ReadWrite;
            options.Mode = FileMode.Append;
            options.Access = FileAccess.Write;

            m_writer = new StreamWriter(m_fileName, options);
            File.SetCreationTime(m_fileName, DateTimeOffset.Now.LocalDateTime);
        }

        public void Dispose()
        {
            Close();
        }
    }
}
