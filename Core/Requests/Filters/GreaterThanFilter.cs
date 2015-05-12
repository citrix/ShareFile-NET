namespace ShareFile.Api.Client.Requests.Filters
{
    public class GreaterThanFilter : IFilter
    {
        public string PropertyName { get; set; }
        public string Value { get; set; }
        
        public GreaterThanFilter(string propertyName, string value)
        {
            PropertyName = propertyName;
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("{0} gt {1}", PropertyName, Value);
        }
    }
}
