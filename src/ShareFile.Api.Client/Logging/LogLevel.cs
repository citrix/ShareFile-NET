using System;

namespace ShareFile.Api.Client.Logging
{
    [Flags]
    public enum LogLevel
    {
        None = 0,
        Trace = 1 << 0,
        Debug = 1 << 1,
        Info = 1 << 2,
        Warn = 1 << 3,
        Error = 1 << 4,
        Fatal = 1 << 5
    }
}
