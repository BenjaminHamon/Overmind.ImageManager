using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using System.IO;
using System.Text;

namespace Overmind.ImageManager.Model
{
	public static class Application
	{
		public const string Identifier = "Overmind.ImageManager";

		public static void InitializeLogging(string applicationFullName, string applicationName, string applicationDataDirectory)
		{
			InternalLogger.LogToConsole = true;
			InternalLogger.LogFile = Path.Combine(Path.GetTempPath(), applicationFullName + ".nlog.log");
			InternalLogger.LogLevel = LogLevel.Warn;

			LoggingConfiguration loggingConfiguration = new LoggingConfiguration();
			string loggingLayout = "${date:format=s} [${level}][${logger}] ${message}${onexception:${newline}${exception:format=ToString}";

			DebuggerTarget debuggerTarget = new DebuggerTarget() { Layout = loggingLayout };
			loggingConfiguration.AddRule(LogLevel.Debug, LogLevel.Fatal, debuggerTarget);

			FileTarget fileTarget = new FileTarget() { Layout = loggingLayout, Encoding = Encoding.UTF8 };
			fileTarget.FileName = Path.Combine(applicationDataDirectory, applicationName + ".log");
			loggingConfiguration.AddRule(LogLevel.Info, LogLevel.Fatal, fileTarget);

			LogManager.Configuration = loggingConfiguration;
		}
	}
}
