namespace ShareFile.Api.Client.Requests.Filters
{
    public abstract class BooleanFilter : IFilter
    {
        protected readonly string binaryOperator;
        public IFilter Left { get; private set; }
        public IFilter Right { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanFilter"/> class.
        /// </summary>
        /// <param name="left">Filter for the left hand side of binaryOperation</param>
        /// <param name="right">Filter for the right hand side of binaryOperation</param>
        /// <param name="binaryOperator">Desired binary operator</param>
        protected BooleanFilter(IFilter left, IFilter right, string binaryOperator)
        {
            this.Left = left;
            this.Right = right;
            this.binaryOperator = binaryOperator;
        }

        /// <summary>
        /// Compute the filter for use.
        /// </summary>
        /// <returns>Constructed filter with <see cref="Left"/> and <see cref="Right"/> filters applied</returns>
        public override string ToString()
        {
            return string.Format("{0} {1} {2}", this.Left, this.binaryOperator, this.Right);
        }
    }
}
