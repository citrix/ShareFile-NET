namespace ShareFile.Api.Client.Requests.Filters
{
    public class NotEqualToFilter : IFilter
    {
        public string PropertyName { get; set; }
        public string Value { get; set; }

        public NotEqualToFilter(string propertyName, string value)
            : this(propertyName, new Filter.Value(value))
        { }

        public NotEqualToFilter(string propertyName, Filter.Value value)
        {
            PropertyName = propertyName;
            Value = value.ToString();
        }

        public override string ToString()
        {
            return string.Format("{0} ne {1}", PropertyName, Value);
        }
    }
}
