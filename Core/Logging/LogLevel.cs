using System;

namespace ShareFile.Api.Client.Logging
{
    [Flags]
    public enum LogLevel
    {
        None = 0x0,
        Trace = 0x1,
        Debug = 0x2,
        Info = 0x4,
        Warn = 0x8,
        Error = 0x10,
        Fatal = 0x20
    }
}
