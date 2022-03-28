using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace DbActivities
{
    /// <summary>
    /// Wraps an existing <see cref="DbConnection"/> and adds
    /// instrumentation using the <see cref="Activity"/> class.
    /// </summary>
    public class InstrumentedDbConnection : DbConnection
    {
        private readonly Activity _connectionActivity;

        private readonly DbConnection _innerDbConnection;

        private readonly InstrumentationOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstrumentedDbConnection"/> class.
        /// </summary>
        /// <param name="dbConnection">The underlying <see cref="DbConnection"/>.</param>
        /// <param name="options"></param>
        public InstrumentedDbConnection(DbConnection dbConnection, InstrumentationOptions options)
        {
            _options = options;
            _innerDbConnection = dbConnection;
            _connectionActivity = options.StartActivity($"{nameof(InstrumentedDbConnection)}");
            _connectionActivity?.AddTag(OpenTelemetrySemanticNames.DbSystem, _options.System);
            _connectionActivity?.AddTag(OpenTelemetrySemanticNames.DbUser, _options.User);
        }

        /// <inheritdoc/>
        public override string ConnectionString { get => _innerDbConnection.ConnectionString; set => _innerDbConnection.ConnectionString = value; }

        /// <inheritdoc/>
        public override int ConnectionTimeout => _innerDbConnection.ConnectionTimeout;

        /// <inheritdoc/>
        public override string Database => _innerDbConnection.Database;

        /// <inheritdoc/>
        public override ConnectionState State => InnerDbConnection.State;

        /// <summary>
        /// Get the inner <see cref="DbConnection"/> being instrumented.
        /// </summary>
        public DbConnection InnerDbConnection { get => _innerDbConnection; }

        /// <inheritdoc/>
        public override string DataSource => _innerDbConnection.DataSource;

        /// <inheritdoc/>
        public override string ServerVersion => _innerDbConnection.ServerVersion;

        /// <summary>
        /// Starts a new <see cref="DbTransaction"/> wrapped inside a <see cref="InstrumentedDbTransaction"/>.
        /// </summary>
        /// <param name="il">The isolation level to be used for the transaction.</param>
        /// <returns>A <see cref="InstrumentedDbTransaction"/> object.</returns>
        protected override DbTransaction BeginDbTransaction(IsolationLevel il)
        {
            var transaction = _innerDbConnection.BeginTransaction(il);
            return new InstrumentedDbTransaction(transaction, this, _options);
        }

        /// <inheritdoc/>
        public override void ChangeDatabase(string databaseName) => _innerDbConnection.ChangeDatabase(databaseName);

        /// <inheritdoc/>
        public override void Close() => _innerDbConnection.Close();

        /// <summary>
        /// Creates and returns a <see cref="InstrumentedDbCommand"/> object associated with the current connection.
        /// </summary>
        /// <returns>A <see cref="InstrumentedDbCommand"/> object.</returns>
        protected override DbCommand CreateDbCommand()
        {
            var command = _innerDbConnection.CreateCommand();
            return new InstrumentedDbCommand(command, this, _options);
        }

        /// <inheritdoc/>
        public override void Open() => _innerDbConnection.Open();

        /// <summary>
        /// Disposes the underlying <see cref="DbConnection"/> and ends the <see cref="Activity"/>.
        /// </summary>
        /// <param name="disposing">true when disposing both unmanaged and managed ressources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _connectionActivity?.SetTag(OpenTelemetrySemanticNames.DbConnectionString, ConnectionString);
                _options.ConfigureActivityInternal(_connectionActivity);
                _options.ConfigureConnectionActivityInternal(_connectionActivity, _innerDbConnection);
                _connectionActivity?.Dispose();
                _innerDbConnection.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
