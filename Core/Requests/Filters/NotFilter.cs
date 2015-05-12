namespace ShareFile.Api.Client.Requests.Filters
{
    public class NotFilter : IFilter
    {
        public IFilter Filter { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanFilter"/> class.
        /// </summary>
        /// <param name="Filter">Filter to apply negate operation</param>
        public NotFilter(IFilter filter)
        {
            this.Filter = filter;
        }

        /// <summary>
        /// Compute the filter for use.
        /// </summary>
        /// <returns>Constructed filter with <see cref="Filter"/> filter applied</returns>
        public override string ToString()
        {
            return string.Format("not({0})", this.Filter);
        }
    }
}
