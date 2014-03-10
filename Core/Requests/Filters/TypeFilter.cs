using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Requests.Filters
{
    public class TypeFilter : IFilter
    {
        public string Type { get; set; }

        public TypeFilter(string type)
        {
            Type = type;
        }

        public override string ToString()
        {
            return string.Format("isof('{0}')", Type);
        }
    }

    public class TypeFilter<T> : IFilter
        where T : ODataObject
    {
        public override string ToString()
        {
            return string.Format("isof('{0}')", typeof(T).FullName);
        }
    }
}
