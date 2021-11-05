namespace DbActivities
{
    /// <summary>
    /// https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/database.md#call-level-attributes
    /// </summary>
    public static class OpenTelemetrySemanticNames
    {
        public const string DbName = "db.statement";

        public const string DbStatement = "db.statement";

        public const string DbOperation = "db.operation";

        public const string DbSystem = "db.system";

        public const string DbConnectionString = "db.connection_string";

        public const string DbUser = "db.user";

        public const string ExceptionEventName = "exception";

        public const string ExceptionType = "exception.type";

        public const string ExceptionMessage = "exception.message";

        public const string ExceptionStackTrace = "exception.stacktrace";
    }

    public static class CustomTagNames
    {
        public const string RowsAffected = "db.rows_affected";

        public const string RowsRead = "db.rows_read";

        public const string ExceptionSource = "exception.source";
    }
}