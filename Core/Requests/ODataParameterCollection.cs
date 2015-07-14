using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShareFile.Api.Client.Requests
{
    public class ODataAction
    {
        public string ActionName { get; set; }

        public ODataParameterCollection Parameters { get; set; }

        public ODataAction()
        {
            Parameters = new ODataParameterCollection();
        }
    }

    public class ODataParameterCollection : HashSet<ODataParameter>
    {
        public string ToStringForUri()
        {
            if (Count == 0) return string.Empty;

            var stringBuilder = new StringBuilder();
            foreach (var parameter in this)
            {
                stringBuilder.AppendFormat("{0},", parameter.ToStringForUri());
            }

            return stringBuilder.Remove(stringBuilder.Length - 1, 1).ToString();
        }

        public override string ToString()
        {
            if (Count == 0) return string.Empty;

            var stringBuilder = new StringBuilder();
            foreach (var parameter in this)
            {
                stringBuilder.AppendFormat("{0},", parameter);
            }

            return stringBuilder.Remove(stringBuilder.Length - 1, 1).ToString();
        }

        internal void AddOrUpdate(ODataParameter parameter)
        {
            if (Contains(parameter))
            {
                Remove(parameter);
            }

            Add(parameter);
        }
    }
}
