using System;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.utkaka.PsdSynchronization.Editor.Psd {
	internal class DummyLogger : ILogger {
		public ILogHandler logHandler { get; set; }
		public bool logEnabled { get; set; }
		public LogType filterLogType { get; set; }
		
		public void Log(LogType logType, Stream stream, string message) { }
		public void LogFormat(LogType logType, Object context, string format, params object[] args) { }

		public void LogException(Exception exception, Object context) { }

		public bool IsLogTypeAllowed(LogType logType) => false;

		public void Log(LogType logType, object message) { }

		public void Log(LogType logType, object message, Object context) { }

		public void Log(LogType logType, string tag, object message) { }

		public void Log(LogType logType, string tag, object message, Object context) { }

		public void Log(object message) { }

		public void Log(string tag, object message) { }

		public void Log(string tag, object message, Object context) { }

		public void LogWarning(string tag, object message) { }

		public void LogWarning(string tag, object message, Object context) { }

		public void LogError(string tag, object message) { }

		public void LogError(string tag, object message, Object context) { }

		public void LogFormat(LogType logType, string format, params object[] args) { }

		public void LogException(Exception exception) { }
	}
}