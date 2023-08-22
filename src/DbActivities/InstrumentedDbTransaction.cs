using System.Data;
using System.Data.Common;
using System.Diagnostics;
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

        public InstrumentedDbTransaction(DbTransaction dbTransaction, InstrumentedDbConnection dbConnection, InstrumentationOptions options)
        {
            _innerDbTransaction = dbTransaction;
            _transactionActivity = options.StartActivity(nameof(InstrumentedDbTransaction));
            _dbConnection = dbConnection;
            _options = options;
        }

        protected override DbConnection DbConnection => _dbConnection;

        public override IsolationLevel IsolationLevel => _innerDbTransaction.IsolationLevel;

        public DbTransaction InnerDbTransaction { get => _innerDbTransaction; }

        /// <inheritdoc/>
        public override void Commit()
        {
            _innerDbTransaction.Commit();
            _transactionActivity?.AddEvent(new ActivityEvent(nameof(Commit)));
        }

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
            await base.RollbackAsync(cancellationToken);
            _transactionActivity?.AddEvent(new ActivityEvent(nameof(RollbackAsync)));
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _options.ConfigureActivityInternal(_transactionActivity);
                _options.ConfigureTransactionActivityInternal(_transactionActivity, _innerDbTransaction);
                _innerDbTransaction.Dispose();
                _transactionActivity?.Dispose();
            }
            base.Dispose(isDisposing);
        }

        public override async ValueTask DisposeAsync()
        {
            _options.ConfigureActivityInternal(_transactionActivity);
            _options.ConfigureTransactionActivityInternal(_transactionActivity, _innerDbTransaction);
            await _innerDbTransaction.DisposeAsync();
            _transactionActivity?.Dispose();

            await base.DisposeAsync();
        }


    }
}
