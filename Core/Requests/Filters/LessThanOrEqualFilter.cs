namespace ShareFile.Api.Client.Requests.Filters
{
    public class LessThanOrEqualFilter : IFilter
    {
        public string PropertyName { get; set; }
        public string Value { get; set; }
                
        public LessThanOrEqualFilter(string propertyName, string value)
        {
            PropertyName = propertyName;
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("{0} le {1}", PropertyName, Value);
        }
    }
}
