using System;
using System.Runtime.CompilerServices;

namespace ShareFile.Api.Client.Logging
{
    internal class LoggingProvider
    {
        private readonly ILogger _instance;
        public bool IsTraceEnabled { get { return _instance != null && _instance.LogLevel.HasFlag(LogLevel.Trace); } }
        public bool IsDebugEnabled { get { return _instance != null && _instance.LogLevel.HasFlag(LogLevel.Debug); } }
        public bool IsInformationEnabled { get { return _instance != null && _instance.LogLevel.HasFlag(LogLevel.Info); } }
        public bool IsWarningEnabled { get { return _instance != null && _instance.LogLevel.HasFlag(LogLevel.Warn); } }
        public bool IsErrorEnabled { get { return _instance != null && _instance.LogLevel.HasFlag(LogLevel.Error); } }
        public bool IsFatalEnabled { get { return _instance != null && _instance.LogLevel.HasFlag(LogLevel.Fatal); } }

        public LoggingProvider(ILogger logger)
        {
            if (logger != null)
            {
                _instance = logger;
            }
        }

        public void Trace(ActionStopwatch stopwatch)
        {
            if (IsTraceEnabled)
            {
                stopwatch.Stop();
                Trace(stopwatch.StringFormat(), stopwatch.Name, stopwatch.ElapsedMilliseconds());
            }
        }

        public void Trace(string format, params object[] args)
        {
            if (IsTraceEnabled)
            {
                _instance.Trace(BuildString(format, args));
            }
        }

        public void Trace(Exception exception, string format, params object[] args)
        {
            if (IsTraceEnabled)
            {
                _instance.Trace(exception, BuildString(format, args));
            }
        }

        public void Debug(string format, params object[] args)
        {
            if (IsDebugEnabled)
            {
                _instance.Debug(BuildString(format, args));
            }
        }

        public void Debug(Exception exception, string format, params object[] args)
        {
            if (IsDebugEnabled)
            {
                _instance.Debug(exception, BuildString(format, args));
            }
        }

        public void Info(string format, params object[] args)
        {
            if (IsInformationEnabled)
            {
                _instance.Info(BuildString(format, args));
            }
        }

        public void Info(Exception exception, string format, params object[] args)
        {
            if (IsInformationEnabled)
            {
                _instance.Info(exception, BuildString(format, args));
            }
        }

        public void Warn(string format, params object[] args)
        {
            if (IsWarningEnabled)
            {
                _instance.Warn(BuildString(format, args));
            }
        }

        public void Warn(Exception exception, string format, params object[] args)
        {
            if (IsWarningEnabled)
            {
                _instance.Warn(exception, BuildString(format, args));
            }
        }

        public void Error(string format, params object[] args)
        {
            if (IsErrorEnabled)
            {
                _instance.Error(BuildString(format, args));
            }
        }

        public void Error(Exception exception, string format, params object[] args)
        {
            if (IsErrorEnabled)
            {
                if (string.IsNullOrEmpty(format))
                {
                    _instance.Error(exception, "");
                }
                else _instance.Error(exception, BuildString(format, args));
            }
        }

        public void Fatal(string format, params object[] args)
        {
            if (IsFatalEnabled)
            {
                _instance.Fatal(BuildString(format, args));
            }
        }

        public void Fatal(Exception exception, string format, params object[] args)
        {
            if (IsFatalEnabled)
            {
                _instance.Fatal(exception, BuildString(format, args));
            }
        }

        private string BuildString(string format, params object[] args)
        {
            if (args == null || args.Length == 0)
                return format;
            return string.Format(format, args);
        }
    }
}
