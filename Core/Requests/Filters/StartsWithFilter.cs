namespace ShareFile.Api.Client.Requests.Filters
{
    public class StartsWithFilter : EqualToFilter
    {
        public StartsWithFilter(string propertyName, string value, bool isEqual = true) :
            base("startswith", propertyName, value, isEqual)
        {
        }
    }
}
