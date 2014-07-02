using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShareFile.Api.Client.Requests.Filters
{
    public class EndsWithFilter : EqualToFilter
    {
        public EndsWithFilter(string propertyName, string value, bool isEqual = true) :
            base ("endswith", propertyName, value, isEqual)
        {
        }
    }
}
