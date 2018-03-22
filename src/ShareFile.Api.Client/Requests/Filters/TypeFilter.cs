using ShareFile.Api.Client.Converters;
using ShareFile.Api.Client.Models;

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
            string fullName = typeof(T).FullName;
            if(fullName.Contains(ODataFactory.ClientODataObjectNamespace))
            {
                fullName = fullName.Replace(ODataFactory.ClientODataObjectNamespace, ODataFactory.PlaftormODataObjectNamespace);
            }
            return $"isof('{fullName}')";
        }
    }
}
