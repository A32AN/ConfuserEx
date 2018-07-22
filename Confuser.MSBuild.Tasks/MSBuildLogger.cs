﻿using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using ILogger = Confuser.Core.ILogger;

namespace Confuser.MSBuild.Tasks {
	internal sealed class MSBuildLogger : ILogger {
		private readonly TaskLoggingHelper loggingHelper;

		internal MSBuildLogger(TaskLoggingHelper loggingHelper) =>
			this.loggingHelper = loggingHelper ?? throw new ArgumentNullException(nameof(loggingHelper));

		void ILogger.Debug(string msg) => loggingHelper.LogMessage(MessageImportance.Low, "[DEBUG] " + msg);

		void ILogger.DebugFormat(string format, params object[] args) {
			loggingHelper.LogMessage(MessageImportance.Low, "[DEBUG] " + format, args);
		}

		void ILogger.EndProgress() {}

		void ILogger.Error(string msg) =>
			loggingHelper.LogError(msg);

		void ILogger.ErrorException(string msg, Exception ex) {
			loggingHelper.LogError(msg);
			loggingHelper.LogErrorFromException(ex);
		}

		void ILogger.ErrorFormat(string format, params object[] args) => loggingHelper.LogError(format, args);

		void ILogger.Finish(bool successful) {}

		void ILogger.Info(string msg) => loggingHelper.LogMessage(MessageImportance.Normal, msg);

		void ILogger.InfoFormat(string format, params object[] args) =>
			loggingHelper.LogMessage(MessageImportance.Normal, format, args);

		void ILogger.Progress(int progress, int overall) { }

		void ILogger.Warn(string msg) => loggingHelper.LogWarning(msg);

		void ILogger.WarnException(string msg, Exception ex) {
			loggingHelper.LogWarning(msg);
			loggingHelper.LogWarningFromException(ex);
		}

		void ILogger.WarnFormat(string format, params object[] args) => loggingHelper.LogWarning(format, args);
	}
}
