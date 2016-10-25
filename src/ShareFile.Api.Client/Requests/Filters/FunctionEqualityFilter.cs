using ShareFile.Api.Client.Extensions;

namespace ShareFile.Api.Client.Requests.Filters
{
    public abstract class FunctionEqualityFilter : IFilter
    {
        public string PropertyName { get; set; }
        public string Value { get; set; }
        public bool IsEqual { get; set; }
        protected string Function { get; set; }

        protected FunctionEqualityFilter(string function, string propertyName, string value, bool isEqual = true)
        {
            Function = function;
            PropertyName = propertyName;
            Value = value;
            IsEqual = isEqual;
        }

        protected string PropertyThenValue()
        {
            return string.Format("{0}({1}, '{2}') eq {3}", Function, PropertyName, Value, IsEqual.ToLowerString());
        }

        protected string ValueThenProperty()
        {
            return string.Format("{0}('{1}', {2}) eq {3}", Function, Value, PropertyName, IsEqual.ToLowerString());
        }
    }
}
