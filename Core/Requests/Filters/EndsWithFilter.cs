namespace ShareFile.Api.Client.Requests.Filters
{
    public class EndsWithFilter : FunctionEqualityFilter
    {
        public EndsWithFilter(string propertyName, string value, bool isEqual = true) :
            base ("endswith", propertyName, value, isEqual)
        {
        }

        public override string ToString()
        {
            return this.PropertyThenValue();
        }
    }
}
