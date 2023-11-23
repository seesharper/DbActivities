using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;

namespace DbActivities
{
    public class InstrumentedDbTransaction : DbTransaction
    {
        private readonly DbTransaction _innerDbTransaction;
        private readonly Activity _transactionActivity;
        private readonly InstrumentedDbConnection _dbConnection;
        private readonly InstrumentationOptions _options;
        private static readonly Histogram<long> s_transactionDuration = InstrumentationOptions.Meter.CreateHistogram<long>("db.client.transactions.use_time", "ms");

        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        /// <summary>
        /// Initializes a new instance of the <see cref="InstrumentedDbTransaction"/> class.
        /// </summary>
        /// <param name="dbTransaction">The <see cref="DbTransaction"/> being instrumented.</param>
        /// <param name="dbConnection">The <see cref="InstrumentedDbConnection"/> associated with this transaction.</param>
        /// <param name="options">The <see cref="InstrumentationOptions"/> to be used when instrumenting.</param>
        public InstrumentedDbTransaction(DbTransaction dbTransaction, InstrumentedDbConnection dbConnection, InstrumentationOptions options)
        {
            _innerDbTransaction = dbTransaction;
            _transactionActivity = options.StartActivity(nameof(InstrumentedDbTransaction));
            _dbConnection = dbConnection;
            _options = options;
        }
        /// <inheritdoc/>
        protected override DbConnection DbConnection => _dbConnection;

        /// <inheritdoc/>
        public override IsolationLevel IsolationLevel => _innerDbTransaction.IsolationLevel;

        /// <inheritdoc/>
        public DbTransaction InnerDbTransaction { get => _innerDbTransaction; }

        /// <inheritdoc/>
        public override void Commit()
        {
            _innerDbTransaction.Commit();
            _transactionActivity?.AddEvent(new ActivityEvent(nameof(Commit)));
        }

        /// <inheritdoc/>
        public override async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            await _innerDbTransaction.CommitAsync(cancellationToken);
            _transactionActivity?.AddEvent(new ActivityEvent(nameof(CommitAsync)));
        }

        /// <inheritdoc/>
        public override void Rollback()
        {
            _innerDbTransaction.Rollback();
            _transactionActivity?.AddEvent(new ActivityEvent(nameof(Rollback)));
        }

        /// <inheritdoc/>
        public override async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            await _innerDbTransaction.RollbackAsync(cancellationToken);
            _transactionActivity?.AddEvent(new ActivityEvent(nameof(RollbackAsync)));
        }

        /// <inheritdoc/>
        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _options.ConfigureActivityInternal(_transactionActivity);
                _options.ConfigureTransactionActivityInternal(_transactionActivity, _innerDbTransaction);
                _innerDbTransaction.Dispose();
                _transactionActivity?.Dispose();
                s_transactionDuration.Record(_stopwatch.ElapsedMilliseconds);

            }
            base.Dispose(isDisposing);
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            _options.ConfigureActivityInternal(_transactionActivity);
            _options.ConfigureTransactionActivityInternal(_transactionActivity, _innerDbTransaction);
            await _innerDbTransaction.DisposeAsync();
            _transactionActivity?.Dispose();
            s_transactionDuration.Record(_stopwatch.ElapsedMilliseconds);
            await base.DisposeAsync();
        }
    }
}
