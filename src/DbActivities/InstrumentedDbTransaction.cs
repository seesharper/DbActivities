using System.Data;
using System.Data.Common;
using System.Diagnostics;

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

        /// <inheritdoc/>
        public override void Rollback()
        {
            _innerDbTransaction.Rollback();
            _transactionActivity?.AddEvent(new ActivityEvent(nameof(Rollback)));
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
    }
}
