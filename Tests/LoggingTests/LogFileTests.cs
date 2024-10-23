using LoggingLibInterface;
using System.Reflection;

namespace LoggingTests
{
    public class LogFileTests
    {
        private string m_logDir;
        private LogChannelFile m_logChannelFile;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            string asmDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            m_logDir = Path.Combine(asmDir, "logs");

            if (Directory.Exists(m_logDir))
                Directory.Delete(m_logDir, true);
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
                Feedback.Instance.UnregisterChannel(m_logChannelFile);
        }

        [Test]
        public void WriteLogFileMessages()
        {
            string logfilePath = Path.Combine(m_logDir, "test.log");
            m_logChannelFile = new LogChannelFile(logfilePath);
            Feedback.Instance.RegisterChannel(m_logChannelFile);

            Feedback.LogUserMessage("This is a user message");
            Feedback.LogInfo("This is an info message");
            Feedback.LogWarning("This is a warning message");
            Feedback.LogError("This is an error message");

            m_logChannelFile.Close();
            Assert.That(File.Exists(logfilePath));
        }

        [Test]
        public void CheckFileBackup()
        {
            string logfilePath = Path.Combine(m_logDir, "test.log");

            File.Create(logfilePath).Close();
            File.Create(logfilePath + ".1").Close();
            File.Create(logfilePath + ".2").Close();

            m_logChannelFile = new LogChannelFile(logfilePath, 2);
            Feedback.Instance.RegisterChannel(m_logChannelFile);

            Feedback.LogUserMessage("This is a user message");
            Feedback.LogInfo("This is an info message");
            Feedback.LogWarning("This is a warning message");
            Feedback.LogError("This is an error message");

            Assert.That(File.Exists(logfilePath));
            Assert.That(File.Exists(logfilePath + ".1"));
            Assert.That(File.Exists(logfilePath + ".2"));
            Assert.That(!File.Exists(logfilePath + ".3"));
        }
    }
}