using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace DbActivities.Tests
{
    [Collection("ActivityTests")]
    public class DbConnectionTests
    {
        [Fact]
        public void ShouldCallOpen()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            var instrumentedDbConnection = dbConnectionMock.Object.AsInstrumentedDbConnection(options => options.ConfigureActivityStarter((name, kind) => null));
            instrumentedDbConnection.Open();
            dbConnectionMock.Verify(m => m.Open());
        }

        [Fact]
        public async Task ShouldCallOpenAsync()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            var instrumentedDbConnection = dbConnectionMock.Object.AsInstrumentedDbConnection(options => options.ConfigureActivityStarter((name, kind) => null));
            await instrumentedDbConnection.OpenAsync(It.IsAny<CancellationToken>());
            dbConnectionMock.Verify(m => m.OpenAsync(It.IsAny<CancellationToken>()));
        }

        [Fact]
        public void ShouldCallClose()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            var instrumentedDbConnection = dbConnectionMock.Object.AsInstrumentedDbConnection(options => options.ConfigureActivityStarter((name, kind) => null));
            instrumentedDbConnection.Close();
            dbConnectionMock.Verify(m => m.Close());
        }

        [Fact]
        public async Task ShouldCallCloseAsync()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            var instrumentedDbConnection = dbConnectionMock.Object.AsInstrumentedDbConnection(options => options.ConfigureActivityStarter((name, kind) => null));
            await instrumentedDbConnection.CloseAsync();
            dbConnectionMock.Verify(m => m.CloseAsync());
        }

        [Fact]
        public void ShouldCallChangeDatabase()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            var instrumentedDbConnection = dbConnectionMock.Object.AsInstrumentedDbConnection(options => options.ConfigureActivityStarter((name, kind) => null));
            instrumentedDbConnection.ChangeDatabase("Northwind");
            dbConnectionMock.Verify(m => m.ChangeDatabase("Northwind"));
        }

        [Fact]
        public void ShouldGetServerVersion()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.SetupGet(m => m.ServerVersion).Returns("1.0.0");
            var instrumentedDbConnection = dbConnectionMock.Object.AsInstrumentedDbConnection(options => options.ConfigureActivityStarter((name, kind) => null));
            var version = instrumentedDbConnection.ServerVersion;
            version.Should().Be("1.0.0");
        }

        [Fact]
        public void ShouldGetDataSource()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.SetupGet(m => m.DataSource).Returns("mySource");
            var instrumentedDbConnection = dbConnectionMock.Object.AsInstrumentedDbConnection(options => options.ConfigureActivityStarter((name, kind) => null));
            var dataSource = instrumentedDbConnection.DataSource;
            dataSource.Should().Be("mySource");
        }

        [Fact]
        public void ShouldSetAndGetConnectionString()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.SetupProperty(m => m.ConnectionString);
            var instrumentedDbConnection = dbConnectionMock.Object.AsInstrumentedDbConnection(options => options.ConfigureActivityStarter((name, kind) => null));
            instrumentedDbConnection.ConnectionString = "myConnectionString";
            var connectionString = instrumentedDbConnection.ConnectionString;
            connectionString.Should().Be("myConnectionString");
        }

        [Fact]
        public void ShouldGetConnectionTimeout()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.SetupGet(m => m.ConnectionTimeout).Returns(42);
            var instrumentedDbConnection = dbConnectionMock.Object.AsInstrumentedDbConnection(options => options.ConfigureActivityStarter((name, kind) => null));
            var connectionTimeout = instrumentedDbConnection.ConnectionTimeout;
            connectionTimeout.Should().Be(42);
        }

        [Fact]
        public void ShouldGetDatabase()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.SetupGet(m => m.Database).Returns("Northwind");
            var instrumentedDbConnection = dbConnectionMock.Object.AsInstrumentedDbConnection(options => options.ConfigureActivityStarter((name, kind) => null));
            var database = instrumentedDbConnection.Database;
            database.Should().Be("Northwind");
        }

        [Fact]
        public void ShouldGetState()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.SetupGet(m => m.State).Returns(ConnectionState.Connecting);
            var instrumentedDbConnection = dbConnectionMock.Object.AsInstrumentedDbConnection(options => options.ConfigureActivityStarter((name, kind) => null));
            var state = instrumentedDbConnection.State;
            state.Should().Be(ConnectionState.Connecting);
        }

        [Fact]
        public void ShouldBeAbleToRaiseEvents()
        {
            var dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.Setup(m => m.Open()).Raises(m => m.StateChange += null, new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));
            using (var connection = new InstrumentedDbConnection(dbConnectionMock.Object, new InstrumentationOptions()))
            {
                ((bool)typeof(InstrumentedDbConnection).GetProperty("CanRaiseEvents", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(connection)).Should().BeTrue();
                connection.StateChange += (s, a) =>
                {
                    a.CurrentState.Should().Be(ConnectionState.Open);
                };
                connection.Open();
            }
        }

        [Fact]
        public void ShouldForwardChangeDatabaseToDecoratedConnection()
        {
            var dbConnectionMock = new Mock<DbConnection>();

            using (var connection = new InstrumentedDbConnection(dbConnectionMock.Object, new()))
            {
                connection.ChangeDatabase(string.Empty);
            }

            dbConnectionMock.Verify(m => m.ChangeDatabase(string.Empty), Times.Once);
        }

        [Fact]
        public void ShouldForwardChangeDatabaseAsyncToDecoratedConnection()
        {
            var dbConnectionMock = new Mock<DbConnection>();

            using (var connection = new InstrumentedDbConnection(dbConnectionMock.Object, new()))
            {
                connection.ChangeDatabaseAsync(string.Empty, CancellationToken.None);
            }

            dbConnectionMock.Verify(m => m.ChangeDatabaseAsync(string.Empty, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void ShouldForwardEnlistTransactionToDecoratedConnection()
        {
            var dbConnectionMock = new Mock<DbConnection>();

            using (var connection = new InstrumentedDbConnection(dbConnectionMock.Object, new()))
            {
                connection.EnlistTransaction(System.Transactions.Transaction.Current);
            }

            dbConnectionMock.Verify(m => m.EnlistTransaction(null), Times.Once);
        }

        [Fact]
        public void ShouldForwardGetSchemaToDecoratedConnection()
        {
            var dbConnectionMock = new Mock<DbConnection>();

            dbConnectionMock.Setup(m => m.GetSchema()).Returns(new DataTable());

            using (var connection = new InstrumentedDbConnection(dbConnectionMock.Object, new()))
            {
                connection.GetSchema();
            }

            dbConnectionMock.Verify(m => m.GetSchema(), Times.Once);
        }

        [Fact]
        public void ShouldForwardGetSchemaWithCollectionNameToDecoratedConnection()
        {
            var dbConnectionMock = new Mock<DbConnection>();

            dbConnectionMock.Setup(m => m.GetSchema(It.IsAny<string>())).Returns(new DataTable());

            using (var connection = new InstrumentedDbConnection(dbConnectionMock.Object, new()))
            {
                connection.GetSchema(It.IsAny<string>());
            }

            dbConnectionMock.Verify(m => m.GetSchema(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void ShouldForwardGetSchemaWithCollectionNameAndRestrictValuesToDecoratedConnection()
        {
            var dbConnectionMock = new Mock<DbConnection>();

            dbConnectionMock.Setup(m => m.GetSchema(It.IsAny<string>(), It.IsAny<string[]>())).Returns(new DataTable());

            using (var connection = new InstrumentedDbConnection(dbConnectionMock.Object, new()))
            {
                connection.GetSchema(It.IsAny<string>(), It.IsAny<string[]>());
            }

            dbConnectionMock.Verify(m => m.GetSchema(It.IsAny<string>(), It.IsAny<string[]>()), Times.Once);
        }

    }
}