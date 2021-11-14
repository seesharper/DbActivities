using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
namespace DbActivities
{
    /// <summary>
    /// Wraps an existing <see cref="DbCommand"/> and adds
    /// instrumentation using the <see cref="Activity"/> class.
    /// </summary>
    public class InstrumentedDbCommand : DbCommand
    {
        private readonly DbCommand _innerDbCommand;

        private DbConnection _dbConnection;

        private DbTransaction _dbTransaction;

        private readonly InstrumentationOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstrumentedDbCommand"/> class.
        /// </summary>
        /// <param name="dbCommand">The underlying <see cref="DbCommand"/>.</param>
        /// <param name="dbConnection">The <see cref="DbConnection"/> to be associated with this command.</param>
        /// <param name="options">The <see cref="InstrumentationOptions"/> to be used when instrumenting.</param>
        public InstrumentedDbCommand(DbCommand dbCommand, DbConnection dbConnection, InstrumentationOptions options)
        {
            _dbConnection = dbConnection;
            _innerDbCommand = dbCommand;
            _options = options;
            dbCommand.Connection = dbConnection is InstrumentedDbConnection instrumentedDbConnection ? instrumentedDbConnection.InnerDbConnection : dbConnection;
        }

        /// <summary>
        /// Get the inner <see cref="DbCommand"/> being instrumented.
        /// </summary>
        public DbCommand InnerDbCommand { get => _innerDbCommand; }

        /// <inheritdoc/>
        public override string CommandText { get => _innerDbCommand.CommandText; set => _innerDbCommand.CommandText = value; }

        /// <inheritdoc/>
        public override int CommandTimeout { get => _innerDbCommand.CommandTimeout; set => _innerDbCommand.CommandTimeout = value; }

        /// <inheritdoc/>
        public override CommandType CommandType { get => _innerDbCommand.CommandType; set => _innerDbCommand.CommandType = value; }

        /// <inheritdoc/>
        public override bool DesignTimeVisible { get => _innerDbCommand.DesignTimeVisible; set => _innerDbCommand.DesignTimeVisible = value; }

        /// <inheritdoc/>
        public override UpdateRowSource UpdatedRowSource { get => _innerDbCommand.UpdatedRowSource; set => _innerDbCommand.UpdatedRowSource = value; }

        /// <inheritdoc/>
        protected override DbConnection DbConnection
        {
            get => _dbConnection;
            set
            {
                _dbConnection = value;
                _innerDbCommand.Connection = value is InstrumentedDbConnection instrumentedDbConnection ? instrumentedDbConnection.InnerDbConnection : value;
            }
        }

        /// <inheritdoc/>
        protected override DbParameterCollection DbParameterCollection => _innerDbCommand.Parameters;

        /// <inheritdoc/>
        protected override DbTransaction DbTransaction
        {
            get => _dbTransaction;
            set
            {
                _dbTransaction = value;
                _innerDbCommand.Transaction = value is InstrumentedDbTransaction instrumentedDbTransaction ? instrumentedDbTransaction.InnerDbTransaction : value;
            }
        }

        /// <inheritdoc/>
        public override void Cancel() => _innerDbCommand.Cancel();

        /// <inheritdoc/>
        public override int ExecuteNonQuery()
        {
            using (var activity = _options.ActivitySource.StartActivity($"{nameof(InstrumentedDbCommand)}.{nameof(ExecuteNonQuery)}"))
            {
                _options.ConfigureDbCommandInternal(_innerDbCommand);
                AddCallLevelTags(activity, OperationType.NonQuery);
                try
                {
                    var rowsAffected = _innerDbCommand.ExecuteNonQuery();
                    activity?.AddTag(CustomTagNames.RowsAffected, rowsAffected);
                    return rowsAffected;
                }
                catch (Exception ex)
                {
                    activity?.AddExceptionEvent(ex);
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public override async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            using (var activity = _options.StartActivity($"{nameof(InstrumentedDbCommand)}.{nameof(ExecuteNonQueryAsync)}"))
            {
                _options.ConfigureDbCommandInternal(_innerDbCommand);
                AddCallLevelTags(activity, OperationType.NonQuery);
                try
                {
                    var rowsAffected = await _innerDbCommand.ExecuteNonQueryAsync(cancellationToken);
                    activity?.AddTag(CustomTagNames.RowsAffected, rowsAffected);
                    return rowsAffected;
                }
                catch (Exception ex)
                {
                    activity?.AddExceptionEvent(ex);
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public override object ExecuteScalar()
        {
            using (var activity = _options.StartActivity($"{nameof(InstrumentedDbCommand)}.{nameof(ExecuteScalar)}"))
            {
                _options.ConfigureDbCommandInternal(_innerDbCommand);
                AddCallLevelTags(activity, OperationType.Scalar);
                try
                {
                    return _innerDbCommand.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    activity?.AddExceptionEvent(ex);
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public override async Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            using (var activity = _options.StartActivity($"{nameof(InstrumentedDbCommand)}.{nameof(ExecuteScalarAsync)}"))
            {
                _options.ConfigureDbCommandInternal(_innerDbCommand);
                AddCallLevelTags(activity, OperationType.Scalar);
                try
                {
                    return await _innerDbCommand.ExecuteScalarAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    activity?.AddExceptionEvent(ex);
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public override void Prepare() => _innerDbCommand.Prepare();

        /// <inheritdoc/>
        protected override DbParameter CreateDbParameter() => _innerDbCommand.CreateParameter();

        /// <inheritdoc/>
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            using (var activity = _options.StartActivity($"{nameof(InstrumentedDbCommand)}.{nameof(ExecuteReader)}"))
            {
                _options.ConfigureDbCommandInternal(_innerDbCommand);
                AddCallLevelTags(activity, OperationType.Reader);
                try
                {
                    return new InstrumentedDbDataReader(_innerDbCommand.ExecuteReader(behavior), _options); ;
                }
                catch (Exception ex)
                {
                    activity?.AddExceptionEvent(ex);
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            using (var activity = _options.StartActivity($"{nameof(InstrumentedDbCommand)}.{nameof(ExecuteReaderAsync)}"))
            {
                _options.ConfigureDbCommandInternal(_innerDbCommand);
                AddCallLevelTags(activity, OperationType.Reader);
                try
                {
                    var reader = await _innerDbCommand.ExecuteReaderAsync(behavior, cancellationToken);
                    return new InstrumentedDbDataReader(reader, _options);
                }
                catch (Exception ex)
                {
                    activity?.AddExceptionEvent(ex);
                    throw;
                }
            }
        }

        private void AddCallLevelTags(Activity activity, string operation)
        {
            activity?.AddTag(OpenTelemetrySemanticNames.DbStatement, _options.FormatCommandTextInternal(_innerDbCommand));
            activity?.AddTag(OpenTelemetrySemanticNames.DbOperation, operation);
            activity?.AddTag(OpenTelemetrySemanticNames.DbUser, _options.User);
        }
    }
}