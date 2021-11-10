using System.Data.Common;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace DbActivities.Tests
{
    public class DbTransactionTests
    {
        [Fact]
        public void ShouldGetInnerDbTransaction()
        {
            var mock = new Mock<DbTransaction>();
            var instrumentedDbTransaction = CreateInstrumentedDbTransaction(mock.Object);
            instrumentedDbTransaction.InnerDbTransaction.Should().BeSameAs(mock.Object);
        }

        [Fact]
        public void ShouldGetIsolationLevel()
        {
            var mock = new Mock<DbTransaction>();
            mock.SetupGet(m => m.IsolationLevel).Returns(System.Data.IsolationLevel.Snapshot);
            var instrumentedDbTransaction = CreateInstrumentedDbTransaction(mock.Object);
            instrumentedDbTransaction.IsolationLevel.Should().Be(System.Data.IsolationLevel.Snapshot);
        }

        [Fact]
        public void ShouldGetDbConnection()
        {
            var mock = new Mock<DbTransaction>();
            var instrumentedDbTransaction = CreateInstrumentedDbTransaction(mock.Object);
            instrumentedDbTransaction.InnerDbTransaction.Should().BeSameAs(mock.Object);
            instrumentedDbTransaction.Connection.Should().BeOfType<InstrumentedDbConnection>();
        }


        private static InstrumentedDbTransaction CreateInstrumentedDbTransaction(DbTransaction transaction)
        {
            var options = new InstrumentationOptions().ConfigureActivityStarter((source, kind) => null);

            return new InstrumentedDbTransaction(transaction, new InstrumentedDbConnection(new Mock<DbConnection>().Object, options), options);
        }
    }
}