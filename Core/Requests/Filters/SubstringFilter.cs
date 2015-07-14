namespace ShareFile.Api.Client.Requests.Filters
{
    public class SubstringFilter : FunctionEqualityFilter
    {
        public SubstringFilter(string propertyName, string value, bool isEqual = true) :
            base("substringof", propertyName, value, isEqual)
        {
        }

        public override string ToString()
        {
            return this.ValueThenProperty();
        }
    }
}
