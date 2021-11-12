using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace DbActivities
{
    public class InstrumentedDbCommand : DbCommand
    {
        private readonly DbCommand _innerDbCommand;

        private DbConnection _dbConnection;

        private DbTransaction _dbTransaction;

        private readonly InstrumentationOptions _options;

        public InstrumentedDbCommand(DbCommand dbCommand, DbConnection dbConnection, InstrumentationOptions options)
        {
            _dbConnection = dbConnection;
            _innerDbCommand = dbCommand;
            _options = options;
            dbCommand.Connection = dbConnection is InstrumentedDbConnection instrumentedDbConnection ? instrumentedDbConnection.InnerDbConnection : dbConnection;
        }

        public DbCommand InnerDbCommand { get => _innerDbCommand; }

        public override string CommandText { get => _innerDbCommand.CommandText; set => _innerDbCommand.CommandText = value; }
        public override int CommandTimeout { get => _innerDbCommand.CommandTimeout; set => _innerDbCommand.CommandTimeout = value; }
        public override CommandType CommandType { get => _innerDbCommand.CommandType; set => _innerDbCommand.CommandType = value; }
        public override bool DesignTimeVisible { get => _innerDbCommand.DesignTimeVisible; set => _innerDbCommand.DesignTimeVisible = value; }
        public override UpdateRowSource UpdatedRowSource { get => _innerDbCommand.UpdatedRowSource; set => _innerDbCommand.UpdatedRowSource = value; }
        protected override DbConnection DbConnection
        {
            get => _dbConnection;
            set
            {
                _dbConnection = value;
                _innerDbCommand.Connection = value is InstrumentedDbConnection instrumentedDbConnection ? instrumentedDbConnection.InnerDbConnection : value;
            }
        }

        protected override DbParameterCollection DbParameterCollection => _innerDbCommand.Parameters;

        protected override DbTransaction DbTransaction
        {
            get => _dbTransaction;
            set
            {
                _dbTransaction = value;
                _innerDbCommand.Transaction = value is InstrumentedDbTransaction instrumentedDbTransaction ? instrumentedDbTransaction.InnerDbTransaction : value;
            }
        }

        public override void Cancel() => _innerDbCommand.Cancel();
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
                    activity.AddExceptionEvent(ex);
                    throw;
                }
            }
        }

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

        public override void Prepare() => _innerDbCommand.Prepare();
        protected override DbParameter CreateDbParameter() => _innerDbCommand.CreateParameter();
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
