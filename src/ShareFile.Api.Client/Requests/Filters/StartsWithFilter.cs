namespace ShareFile.Api.Client.Requests.Filters
{
    public class StartsWithFilter : FunctionEqualityFilter
    {
        public StartsWithFilter(string propertyName, string value, bool isEqual = true) :
            base("startswith", propertyName, value, isEqual)
        {
        }

        public override string ToString()
        {
            return this.PropertyThenValue();
        }
    }
}
