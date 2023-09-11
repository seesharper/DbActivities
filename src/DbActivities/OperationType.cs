namespace DbActivities
{
    /// <summary>
    /// Describes the type of operation being performed.
    /// </summary>
    public static class OperationType
    {
        /// <summary>
        /// An operation that does not return a result set.
        /// </summary>
        public const string NonQuery = "nonquery";


        /// <summary>
        /// An operation that returns a single value.
        /// </summary>
        public const string Scalar = "scalar";

        /// <summary>
        /// An operation that returns a result set.
        /// </summary>
        public const string Reader = "reader";
    }
}