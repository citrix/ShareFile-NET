namespace ShareFile.Api.Client.Requests.Filters
{
    public class SubstringFilter : EqualToFilter
    {
        public SubstringFilter(string propertyName, string value, bool isEqual = true) :
            base("substringof", propertyName, value, isEqual)
        {
        }
    }
}
