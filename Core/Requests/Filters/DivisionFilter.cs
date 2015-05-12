namespace ShareFile.Api.Client.Requests.Filters
{
    public class DivisionFilter : IFilter
    {
        public string PropertyName { get; set; }
        public string PropertyValue1 { get; set; }
        public string PropertyValue2 { get; set; }
        public string LogicalOperator { get; set; }

        public DivisionFilter(string propertyName, string value1, string logicalOperator, string value2)
        {
            PropertyName = propertyName;
            PropertyValue1 = value1;
            PropertyValue2 = value2;
            LogicalOperator = logicalOperator;
        }

        public override string ToString()
        {
                return string.Format("{0} div '{1}' {2} '{3}'", PropertyName, PropertyValue1, LogicalOperator, PropertyValue2);
        }
    }
}
