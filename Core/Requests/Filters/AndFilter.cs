namespace ShareFile.Api.Client.Requests.Filters
{
    /// <summary>
    /// OData Filter for ANDing two filters
    /// </summary>
    public class AndFilter : BooleanFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AndFilter"/> class.
        /// </summary>
        /// <param name="left">Left hand side of the filter</param>
        /// <param name="right">Right hand side of the filter</param>
        public AndFilter(IFilter left, IFilter right)
            : base(left, right, "and")
        {
        }
    }
}
