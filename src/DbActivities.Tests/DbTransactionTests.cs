using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;

namespace DbActivities.Tests
{
    [Collection("ActivityTests")]
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

        [Fact]
        public void ShouldDisposeTransaction()
        {
            var mock = new Mock<DbTransaction>();
            mock.Protected().Setup("Dispose", ItExpr.IsAny<bool>());
            var instrumentedDbTransaction = CreateInstrumentedDbTransaction(mock.Object);
            instrumentedDbTransaction.Dispose();

            mock.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
        }

        [Fact]
        public async Task ShouldDisposeTransactionAsync()
        {
            var mock = new Mock<DbTransaction>();
            mock.Setup(m => m.DisposeAsync()).Returns(ValueTask.CompletedTask);
            var instrumentedDbTransaction = CreateInstrumentedDbTransaction(mock.Object);
            await instrumentedDbTransaction.DisposeAsync();

            mock.Verify(m => m.DisposeAsync(), Times.Once());
        }


        private static InstrumentedDbTransaction CreateInstrumentedDbTransaction(DbTransaction transaction)
        {
            var options = new InstrumentationOptions().ConfigureActivityStarter((source, kind) => null);

            return new InstrumentedDbTransaction(transaction, new InstrumentedDbConnection(new Mock<DbConnection>().Object, options), options);
        }
    }
}