using System;

namespace ShareFile.Api.Client.Logging
{
    internal class ActionStopwatch
    {
        private readonly IStopwatch _stopwatch;
        public string Name { get; set; }
        private LoggingProvider LoggingProvider { get; set; }

        internal ActionStopwatch(string name, LoggingProvider loggingProvider)
        {
            _stopwatch = StopwatchFactory.GetStopwatch();
            Name = name;
            LoggingProvider = loggingProvider;
            Start();
        }

        public void Start()
        {
            if (LoggingProvider.IsTraceEnabled)
            {
                _stopwatch.Start();
            }
        }

        public void Stop()
        {
            if (LoggingProvider.IsTraceEnabled)
            {
                _stopwatch.Stop();
            }
        }

        public long ElapsedMilliseconds()
        {
            return _stopwatch.ElapsedMilliseconds;
        }

        public string StringFormat()
        {
            return "{0} took {1} ms to execute.";
        }

        public string[] LogVariables()
        {
            return new[] { ElapsedMilliseconds().ToString() };
        }
    }

    internal interface IStopwatch
    {
        void Start();
        void Stop();
        long ElapsedMilliseconds { get; }
        bool IsRunning { get; }
    }

    /// <summary>
    /// Portable implementation of System.Diagnostics.Stopwatch.  If you're platform provides a higher precision implementation, 
    /// most will, just register with the StopwatchFactory.
    /// </summary>
    internal class Stopwatch : IStopwatch
    {
        private const long TicksPerMillisecond = 10000;
        private bool _isRunning;
        private long _startTicks;
        private long _elapsedTicks;
        private TimeSpan _timeSpan;

        public void Start()
        {
            if (!IsRunning)
            {
                _isRunning = true;
                _startTicks = DateTime.UtcNow.Ticks;
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                _isRunning = false;
                _elapsedTicks = DateTime.UtcNow.Ticks;
            }
        }

        public long ElapsedMilliseconds
        {
            get { return GetElapsedTicks() / TicksPerMillisecond; }
        }

        public bool IsRunning { get { return _isRunning; } }

        private long GetElapsedTicks()
        {
            if (IsRunning)
            {
                return DateTime.UtcNow.Ticks - _startTicks;
            }
            return _elapsedTicks - _startTicks;
        }
    }

    internal class StopwatchFactory
    {
        private static Type _stopwatchType;

        public static void RegisterStopwatch<T>()
            where T : IStopwatch
        {
            _stopwatchType = typeof (T);
        }

        public static IStopwatch GetStopwatch()
        {
            if (_stopwatchType == null)
            {
                return new Stopwatch();
            }
            return (IStopwatch) Activator.CreateInstance(_stopwatchType);
        }
    }
}
