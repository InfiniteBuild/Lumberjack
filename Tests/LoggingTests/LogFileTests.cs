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
            Logging.Close();

            if (m_logChannelFile != null)
            {
                m_logChannelFile.Dispose();
                m_logChannelFile = null;
            }
        }

        [Test]
        public void WriteLogFileMessages()
        {
            Assert.That(m_logDir, Is.Not.Null, "Log directory should not be null");

            string logfilePath = Path.Combine(m_logDir, "test.log");
            Logging.CreateLogFile(logfilePath, false);

            Logging.LogUserMessage("This is a user message");
            Logging.LogInfo("This is an info message");
            Logging.LogWarning("This is a warning message");
            Logging.LogError("This is an error message");

            Logging.Close();
            Assert.That(File.Exists(logfilePath));

            string[] lines = File.ReadAllLines(logfilePath, System.Text.Encoding.UTF8);
            Assert.That(lines.Length, Is.GreaterThanOrEqualTo(4), "Expected at least 4 log entries in the file");
        }

        [Test]
        public void CheckFileBackup()
        {
            Assert.That(m_logDir, Is.Not.Null, "Log directory should not be null");

            string logfilePath = Path.Combine(m_logDir, "test.log");

            File.Create(logfilePath).Close();
            File.Create(logfilePath + ".1").Close();
            File.Create(logfilePath + ".2").Close();

            m_logChannelFile = new LogChannelFile(logfilePath, false, 2);
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