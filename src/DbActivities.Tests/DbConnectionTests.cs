using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace DbActivities.Tests
{
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
    }
}