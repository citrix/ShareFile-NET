using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShareFile.Api.Client.Extensions
{
    public static class BoolExtensions
    {
        public static string ToLowerString(this bool value)
        {
            return value.ToString().ToLowerInvariant();
        }
    }
}
