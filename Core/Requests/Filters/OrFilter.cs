namespace ShareFile.Api.Client.Requests.Filters
{
    /// <summary>
    /// OData Filter for ORing two filters
    /// </summary>
    public class OrFilter : BooleanFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrFilter"/> class.
        /// </summary>
        /// <param name="left">Left hand side of the filter</param>
        /// <param name="right">Right hand side of the filter</param>
        public OrFilter(IFilter left, IFilter right)
            : base(left, right, "or")
        {
        }
    }
}
