using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using ShareFile.Api.Client.Models;

namespace ShareFile.Api.Client.Requests.Filters
{
    public interface IFilter
    {
        
    }

    public static class Filter
    {
        public class Operator
        {
            public static Operator Equal = new Operator { Op = (p, v) => new EqualToFilter(p, v) };
            public static Operator NotEqual = new Operator { Op = (p, v) => new NotEqualToFilter(p, v) };
            public static Operator LessThan = new Operator { Op = (p, v) => new LessThanFilter(p, v) };
            public static Operator GreaterThan = new Operator { Op = (p, v) => new GreaterThanFilter(p, v) };

            internal Func<string, Value, IFilter> Op { get; set; }
        }

        public class Function
        {
            public static Value Date(DateTime dt)
            {
                return new Value { String = string.Format("date({0})", new Value(dt)) };
            }

            public static Value Time(DateTime dt)
            {
                return new Value { String = string.Format("time({0})", new Value(dt)) };
            }
        }
        
        public class Value 
        {
            internal string String { get; set; }

            internal Value() { }

            public Value(string unescaped)
            {
                String = string.Format("'{0}'", unescaped);
            }

            public Value(DateTime dt)
            {
                String = dt.ToUniversalTime().ToString("u");
            }

            public override string ToString()
            {
                return String;
            }
        }
    }
    
}
