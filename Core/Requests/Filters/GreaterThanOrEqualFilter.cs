namespace ShareFile.Api.Client.Requests.Filters
{
    public class GreaterThanOrEqualFilter : IFilter
    {
        public string PropertyName { get; set; }
        public string Value { get; set; }

        public GreaterThanOrEqualFilter(string propertyName, string value)
        {
            PropertyName = propertyName;
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("{0} ge {1}", PropertyName, Value);
        }
    }
}
