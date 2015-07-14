﻿namespace ShareFile.Api.Client.Requests.Filters
{
    public class EqualToFilter : IFilter
    {
        public string PropertyName { get; set; }
        public string Value { get; set; }

        public EqualToFilter(string propertyName, string value)
        {
            PropertyName = propertyName;
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("{0} eq '{1}'", PropertyName, Value);
        }
    }
}
