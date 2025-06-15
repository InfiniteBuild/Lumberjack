using System.Collections.Concurrent;
using System.Threading;

namespace Lumberjack.Interface
{
    public class Logging : IDisposable
    {
        private static Logging? m_instance = null;

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
        private ConcurrentQueue<LogEntry> m_logQueue = new ConcurrentQueue<LogEntry>();
        private SemaphoreSlim m_newItemSignal = new SemaphoreSlim(0);
        private CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource();
        private Task? m_processingTask = null;
        private readonly object m_lock = new object();
        private bool m_stopping = false;

        protected Logging()
        {
            Start();
        }

        public void RegisterChannel(ILoggingChannel channel)
        {
            lock (m_lock)
            {
                m_loggingChannels.Add(channel);
            }
        }

        public void UnregisterChannel(ILoggingChannel channel)
        {
            lock(m_lock)
            {
                if (m_loggingChannels.Contains(channel))
                {
                    channel.Close();
                    m_loggingChannels.Remove(channel);
                }
            }
        }

        public void Start()
        {
            if ((m_processingTask != null) && !m_processingTask.IsCompleted)
            {
                Console.WriteLine("Logging is already started.");
                return;
            }

            Console.WriteLine("Starting queue processor...");

            m_processingTask = Task.Factory.StartNew(
            () => ProcessLogEntriesAsync(),
            m_cancellationTokenSource.Token,
            TaskCreationOptions.LongRunning, // Crucial hint for dedicated thread-like behavior
            TaskScheduler.Default).Unwrap();

            Console.WriteLine("Queue processor started.");
        }

        public void Stop()
        {
            if (m_processingTask == null)
            {
                Console.WriteLine("Processor is not running.");
                return;
            }

            m_stopping = true;
            m_newItemSignal.Release(); // Ensure we release the semaphore for any remaining items
            Console.WriteLine("Stopping queue processor...");
            m_cancellationTokenSource.Cancel(); // Signal cancellation
            m_newItemSignal.Release(); // Release the semaphore to unblock the WaitAsync call if it's waiting

            // Wait for the processing task to complete
            m_processingTask.Wait();
            Console.WriteLine("Queue processor stopped.");
        }

        public void LogMessage(LogLevel level, string message)
        {
            LogMessage(level, message, string.Empty);
        }

        public void LogMessage(LogLevel level, string message, string component)
        {
            if (m_stopping)
                return;

            LogEntry logEntry = new LogEntry(level, message, component);
            m_logQueue.Enqueue(logEntry);
            m_newItemSignal.Release();
        }

        public void CloseAllLogs()
        {
            Stop();

            lock(m_lock)
            {
                foreach (ILoggingChannel channel in m_loggingChannels)
                {
                    channel.Close();
                }
                m_loggingChannels.Clear();
            }
        }

        public static void LogUserMessage(string message)
        {
            LogUserMessage(message, string.Empty);
        }

        public static void LogUserMessage(string message, string component)
        {
            Instance.LogMessage(LogLevel.UserInfo, message, component);
        }

        public static void LogInfo(string message)
        {
            LogInfo(message, string.Empty);
        }

        public static void LogInfo(string message, string component)
        {
            Instance.LogMessage(LogLevel.Info, message, component);
        }

        public static void LogWarning(string message)
        {
            LogWarning(message, string.Empty);
        }

        public static void LogWarning(string message, string component)
        {
            Instance.LogMessage(LogLevel.Warning, message, component);
        }

        public static void LogError(string message)
        {
            LogError(message, string.Empty);
        }

        public static void LogError(string message, string component)
        {
            Instance.LogMessage(LogLevel.Error, message, component);
        }

        public static void Close()
        {
            Instance.CloseAllLogs();
        }

        public static ILoggingChannel CreateLogFile(string filepath, bool reset = false)
        {
            return CreateLogFile(filepath, string.Empty, reset, LogLevel.All);
        }

        public static ILoggingChannel CreateLogFile(string filepath, string component, bool reset = false)
        {
            return CreateLogFile(filepath, component, reset, LogLevel.All);
        }

        public static ILoggingChannel CreateLogFile(string filepath, bool reset, LogLevel filter)
        {
            return CreateLogFile(filepath, string.Empty, reset, filter);
        }

        public static ILoggingChannel CreateLogFile(string filepath, string component, bool reset, LogLevel filter)
        {
            LogChannelFile logChannel = new LogChannelFile(filepath, reset, filter);
            logChannel.ComponentFilter = component;

            Instance.RegisterChannel(logChannel);
            return logChannel;
        }

        public static ILoggingChannel CreateConsoleLog()
        {
            return CreateConsoleLog(LogLevel.UserInfo, LogDisplayFlags.None);
        }

        public static ILoggingChannel CreateConsoleLog(LogLevel filter)
        {
            return CreateConsoleLog(filter, LogDisplayFlags.None);
        }

        public static ILoggingChannel CreateConsoleLog(LogLevel filter, LogDisplayFlags displayFlags)
        {
            LogChannelConsole logChannel = new LogChannelConsole(filter, displayFlags);
            Instance.RegisterChannel(logChannel);
            return logChannel;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_cancellationTokenSource.Cancel();
                m_newItemSignal.Release(); // Ensure the semaphore is released so WaitAsync can unblock and observe cancellation
                if (m_processingTask != null && !m_processingTask.IsCompleted)
                {
                    Console.WriteLine("Waiting for processing task to complete...");
                }
                m_cancellationTokenSource.Dispose();
                m_newItemSignal.Dispose();
            }
        }

        private async Task ProcessLogEntriesAsync()
        {
            CancellationToken cancellationToken = m_cancellationTokenSource.Token;
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await m_newItemSignal.WaitAsync(cancellationToken);

                    while (m_logQueue.TryDequeue(out LogEntry? logEntry))
                    {
                        List<ILoggingChannel> channelsCopy;
                        lock (m_lock)
                        {
                            channelsCopy = new List<ILoggingChannel>(m_loggingChannels);
                        }

                        foreach (var channel in channelsCopy)
                        {
                            channel.LogMessage(logEntry.Level, logEntry.Message, logEntry.Component);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Queue processing cancelled.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An unhandled error occurred in the queue processor: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Queue processing loop exiting.");
            }
        }
    }
}
