using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace DbActivities
{
    /// <summary>
    /// Wraps an existing <see cref="DbConnection"/> and adds
    /// instrumentation using the <see cref="Activity"/> class.
    /// </summary>
    public class InstrumentedDbConnection : DbConnection
    {
        private bool _isDisposed = false;

        private readonly Activity _connectionActivity;

        private readonly DbConnection _innerDbConnection;

        private readonly InstrumentationOptions _options;

        private static readonly UpDownCounter<int> s_openConnections = InstrumentationOptions.Meter.CreateUpDownCounter<int>("db.connections.open");

        private static readonly Histogram<long> s_connectionDuration = InstrumentationOptions.Meter.CreateHistogram<long>("db.client.connections.use_time", "ms");

        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

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
            s_openConnections.Add(1);
            InnerDbConnection.StateChange += StateChangeHandler;
        }

        /// <inheritdoc/>
        public override string ConnectionString { get => _innerDbConnection.ConnectionString; set => _innerDbConnection.ConnectionString = value; }

        /// <inheritdoc/>
        public override int ConnectionTimeout => _innerDbConnection.ConnectionTimeout;

        /// <inheritdoc/>
        public override string Database => _innerDbConnection.Database;

        /// <inheritdoc/>
        public override ConnectionState State => InnerDbConnection.State;

        /// <inheritdoc />
        protected override bool CanRaiseEvents => true;

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
        protected override async ValueTask<DbTransaction> BeginDbTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken)
        {
            var transaction = await _innerDbConnection.BeginTransactionAsync(isolationLevel, cancellationToken);
            return new InstrumentedDbTransaction(transaction, this, _options);
        }

        /// <inheritdoc/>
        public override void ChangeDatabase(string databaseName) => _innerDbConnection.ChangeDatabase(databaseName);

        /// <inheritdoc />
        public override Task ChangeDatabaseAsync(string databaseName, CancellationToken cancellationToken = default)
            => InnerDbConnection.ChangeDatabaseAsync(databaseName, cancellationToken);

        /// <inheritdoc/>
        public override void Close() => _innerDbConnection.Close();

        /// <inheritdoc />
        public override Task CloseAsync()
            => InnerDbConnection.CloseAsync();

        /// <inheritdoc />
        public override void EnlistTransaction(System.Transactions.Transaction transaction)
            => InnerDbConnection.EnlistTransaction(transaction);

        /// <inheritdoc />
        public override DataTable GetSchema()
            => InnerDbConnection.GetSchema();

        /// <inheritdoc />
        public override DataTable GetSchema(string collectionName)
            => InnerDbConnection.GetSchema(collectionName);

        /// <inheritdoc />
        public override DataTable GetSchema(string collectionName, string[] restrictionValues)
            => InnerDbConnection.GetSchema(collectionName, restrictionValues);

        /// <summary>
        /// Creates and returns a <see cref="InstrumentedDbCommand"/> object associated with the current connection.
        /// </summary>
        /// <returns>A <see cref="InstrumentedDbCommand"/> object.</returns>
        protected override DbCommand CreateDbCommand()
        {
            var command = _innerDbConnection.CreateCommand();
            return _options.CommandFactory(command, this, _options);
        }

        /// <inheritdoc/>
        public override void Open() => _innerDbConnection.Open();

        /// <inheritdoc/>
        public override async Task OpenAsync(CancellationToken cancellationToken)
            => await InnerDbConnection.OpenAsync(cancellationToken);

        /// <summary>
        /// Disposes the underlying <see cref="DbConnection"/> and ends the <see cref="Activity"/>.
        /// </summary>
        /// <param name="disposing">true when disposing both unmanaged and managed resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            if (disposing)
            {
                _connectionActivity?.SetTag(OpenTelemetrySemanticNames.DbConnectionString, ConnectionString);
                _options.ConfigureActivityInternal(_connectionActivity);
                _options.ConfigureConnectionActivityInternal(_connectionActivity, _innerDbConnection);
                _connectionActivity?.Dispose();
                InnerDbConnection.StateChange -= StateChangeHandler;
                _innerDbConnection.Dispose();
                s_openConnections.Add(-1);
                s_connectionDuration.Record(_stopwatch.ElapsedMilliseconds);
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public override async ValueTask DisposeAsync()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            _connectionActivity?.SetTag(OpenTelemetrySemanticNames.DbConnectionString, ConnectionString);
            _options.ConfigureActivityInternal(_connectionActivity);
            _options.ConfigureConnectionActivityInternal(_connectionActivity, _innerDbConnection);
            _connectionActivity?.Dispose();
            InnerDbConnection.StateChange -= StateChangeHandler;
            await _innerDbConnection.DisposeAsync();
            s_openConnections.Add(-1);
            s_connectionDuration.Record(_stopwatch.ElapsedMilliseconds);
        }

        private void StateChangeHandler(object sender, StateChangeEventArgs stateChangeEventArguments)
        {
            OnStateChange(stateChangeEventArguments);
        }
    }
}
