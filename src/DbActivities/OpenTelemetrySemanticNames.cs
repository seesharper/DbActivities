namespace DbActivities
{
    /// <summary>
    /// https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/database.md#call-level-attributes
    /// </summary>
    public static class OpenTelemetrySemanticNames
    {
        /// <summary>
        /// The semantic convention for the name of the database being accessed.
        /// </summary>
        public const string DbName = "db.name";

        /// <summary>
        /// The database statement being executed.
        /// </summary>
        public const string DbStatement = "db.statement";

        /// <summary>
        /// The name of the operation being executed. <see cref="OperationType"/>.
        /// </summary>
        public const string DbOperation = "db.operation";

        /// <summary>
        /// An identifier for the database management system (DBMS) product being used. 
        /// </summary>
        public const string DbSystem = "db.system";

        /// <summary>
        /// The connection string used to connect to the database.
        /// </summary>
        public const string DbConnectionString = "db.connection_string";

        /// <summary>
        /// Username for accessing the database.
        /// </summary>
        public const string DbUser = "db.user";

        /// <summary>
        /// The name of he exception event that MUST be "exception".
        /// </summary>
        public const string ExceptionEventName = "exception";

        /// <summary>
        /// The type of the exception.
        /// </summary>
        public const string ExceptionType = "exception.type";

        /// <summary>
        /// The exception message.
        /// </summary>
        public const string ExceptionMessage = "exception.message";

        /// <summary>
        /// A stacktrace as a string in the natural representation for the language runtime.
        /// </summary>
        public const string ExceptionStackTrace = "exception.stacktrace";

        /// <summary>
        /// The number of connections that are currently in state described by the state attribute.
        /// </summary>
        public const string DbConnectionsUsage = "db.client.connections.usage";
    }

    /// <summary>
    /// A set of custom tag names that is not defined by OpenTelemetry.
    /// </summary>
    public static class CustomTagNames
    {
        /// <summary>
        /// The number of rows affected by the query.
        /// </summary>
        public const string RowsAffected = "db.rows_affected";

        /// <summary>
        /// The number of rows read by the query.
        /// </summary>
        public const string RowsRead = "db.rows_read";

        /// <summary>
        /// The source of the exception.
        /// </summary>
        public const string ExceptionSource = "exception.source";
    }
}