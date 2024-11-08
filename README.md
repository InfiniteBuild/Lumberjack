# Lumberjack

Lumberjack is a CSharp library that supports basic application logging.  The key features are multiple logging channels, the ability to create custom channels, and different logging levels.  The idea is you can use the logging library for all messages and feedback and the set the logging channels to determine what entries are stored or displayed in each channel.  For example, there is a console logger command line applications, which by default will display user and error messages whereas the File logger will store all logging messages by default.

## Getting Started
The "Logging" static class is the main interface element.  Use "Logging.CreateLogFile" to create a logfile.  Then use the "LogUserInfo", "LogInfo", "LogWarnign", "LogError" methods to add entries in the logfile.  
