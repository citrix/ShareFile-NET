namespace ShareFile.Api.Client.Requests.Filters
{
    public class LessThanFilter : IFilter
    {
        public string PropertyName { get; set; }
        public string Value { get; set; }

        public LessThanFilter(string propertyName, string value)
        {
            PropertyName = propertyName;
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("{0} lt {1}", PropertyName, Value);
        }
    }
}
