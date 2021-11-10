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

        public InstrumentedDbTransaction(DbTransaction dbTransaction, InstrumentedDbConnection dbConnection, InstrumentationOptions options)
        {
            _innerDbTransaction = dbTransaction;
            _transactionActivity = options.StartActivity(nameof(InstrumentedDbTransaction));
            _dbConnection = dbConnection;
        }

        protected override DbConnection DbConnection => _dbConnection;

        public override IsolationLevel IsolationLevel => _innerDbTransaction.IsolationLevel;

        public DbTransaction InnerDbTransaction { get => _innerDbTransaction; }

        public override void Commit()
        {
            _innerDbTransaction.Commit();
            _transactionActivity?.AddEvent(new ActivityEvent(nameof(Commit)));
        }

        public override void Rollback()
        {
            _innerDbTransaction.Rollback();
            _transactionActivity?.AddEvent(new ActivityEvent(nameof(Rollback)));
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _innerDbTransaction.Dispose();
                _transactionActivity?.Dispose();
            }
            base.Dispose(isDisposing);
        }
    }
}
