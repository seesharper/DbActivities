using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace DbActivities
{
    public class InstrumentedDbConnection : DbConnection
    {
        private readonly Activity _connectionActivity;

        private readonly DbConnection _innerDbConnection;

        private readonly InstrumentationOptions _options;

        public InstrumentedDbConnection(DbConnection dbConnection, InstrumentationOptions options)
        {
            _options = options;
            _innerDbConnection = dbConnection;
            _connectionActivity = options.ActivitySource.StartActivity($"{nameof(InstrumentedDbConnection)}");
            _connectionActivity.AddTag(OpenTelemetrySemanticNames.DbSystem, _options.System);
            _connectionActivity.AddTag(OpenTelemetrySemanticNames.DbUser, _options.User);
        }

        public override string ConnectionString { get => _innerDbConnection.ConnectionString; set => _innerDbConnection.ConnectionString = value; }

        public override int ConnectionTimeout => _innerDbConnection.ConnectionTimeout;

        public override string Database => _innerDbConnection.Database;

        public override ConnectionState State => InnerDbConnection.State;

        public DbConnection InnerDbConnection { get => _innerDbConnection; }

        public override string DataSource => _innerDbConnection.DataSource;

        public override string ServerVersion => _innerDbConnection.DataSource;

        protected override DbTransaction BeginDbTransaction(IsolationLevel il)
        {
            var transaction = _innerDbConnection.BeginTransaction(il);
            return new InstrumentedDbTransaction(transaction, this, _options);
        }

        public override void ChangeDatabase(string databaseName) => _innerDbConnection.ChangeDatabase(databaseName);

        public override void Close() => _innerDbConnection.Close();

        protected override DbCommand CreateDbCommand()
        {
            var command = _innerDbConnection.CreateCommand();
            return new InstrumentedDbCommand(command, this, _options);
        }

        public override void Open()
        {
            _innerDbConnection.Open();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _connectionActivity?.SetTag(OpenTelemetrySemanticNames.DbConnectionString, ConnectionString);
                _connectionActivity?.Dispose();
                _innerDbConnection.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
