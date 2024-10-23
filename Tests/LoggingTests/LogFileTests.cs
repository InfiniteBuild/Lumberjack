using Lumberjack.Interface;
using System.Reflection;

namespace LoggingTests
{
    public class LogFileTests
    {
        private string? m_logDir;
        private LogChannelFile? m_logChannelFile = null;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            string? asmDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrWhiteSpace(asmDir))
                m_logDir = Path.Combine(asmDir, "logs");

            if (Directory.Exists(m_logDir))
                Directory.Delete(m_logDir, true);

            if (string.IsNullOrWhiteSpace(m_logDir))
                throw new ArgumentException("Could not determine log directory");

            Directory.CreateDirectory(m_logDir);
        }

        [SetUp]
        public void Setup()
        {
            
        }

        [TearDown]
        public void TearDown()
        {
            if (m_logChannelFile != null)
            {
                Logging.Instance.UnregisterChannel(m_logChannelFile);
                m_logChannelFile.Dispose();
                m_logChannelFile = null;
            }
        }

        [Test]
        public void WriteLogFileMessages()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            string logfilePath = Path.Combine(m_logDir, "test.log");
#pragma warning restore CS8604 // Possible null reference argument.
            m_logChannelFile = new LogChannelFile(logfilePath);
            Logging.Instance.RegisterChannel(m_logChannelFile);

            Logging.LogUserMessage("This is a user message");
            Logging.LogInfo("This is an info message");
            Logging.LogWarning("This is a warning message");
            Logging.LogError("This is an error message");

            m_logChannelFile.Close();
            Assert.That(File.Exists(logfilePath));
        }

        [Test]
        public void CheckFileBackup()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            string logfilePath = Path.Combine(m_logDir, "test.log");
#pragma warning restore CS8604 // Possible null reference argument.

            File.Create(logfilePath).Close();
            File.Create(logfilePath + ".1").Close();
            File.Create(logfilePath + ".2").Close();

            m_logChannelFile = new LogChannelFile(logfilePath, 2);
            Logging.Instance.RegisterChannel(m_logChannelFile);

            Logging.LogUserMessage("This is a user message");
            Logging.LogInfo("This is an info message");
            Logging.LogWarning("This is a warning message");
            Logging.LogError("This is an error message");

            Assert.That(File.Exists(logfilePath));
            Assert.That(File.Exists(logfilePath + ".1"));
            Assert.That(File.Exists(logfilePath + ".2"));
            Assert.That(!File.Exists(logfilePath + ".3"));
        }
    }
}