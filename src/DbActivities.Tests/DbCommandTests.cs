using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using FluentAssertions;
using Moq;
using Xunit;

namespace DbActivities.Tests
{
    public class DbCommandTests
    {

        [Fact]
        public void ShouldGetInnerDbCommand()
        {
            var mock = new Mock<DbCommand>();
            var instrumentedDbCommand = CreateInstrumentedDbCommand(mock.Object);
            instrumentedDbCommand.InnerDbCommand.Should().BeSameAs(mock.Object);
        }

        [Fact]
        public void ShouldGetAndSetTimeout()
        {
            var mock = new Mock<DbCommand>();
            mock.SetupAllProperties();
            var instrumentinstrumentedDbCommand = CreateInstrumentedDbCommand(mock.Object);
            instrumentinstrumentedDbCommand.CommandTimeout = 10;
            instrumentinstrumentedDbCommand.CommandTimeout.Should().Be(10);
            mock.VerifySet(m => m.CommandTimeout = 10, Times.Once);
            mock.VerifyGet(m => m.CommandTimeout, Times.Once);
        }

        [Fact]
        public void ShouldGetAndSetCommandType()
        {
            var mock = new Mock<DbCommand>();
            mock.SetupAllProperties();
            var instrumentinstrumentedDbCommand = CreateInstrumentedDbCommand(mock.Object);
            instrumentinstrumentedDbCommand.CommandType = CommandType.StoredProcedure;
            instrumentinstrumentedDbCommand.CommandType.Should().Be(CommandType.StoredProcedure);
            mock.VerifySet(m => m.CommandType = CommandType.StoredProcedure, Times.Once);
            mock.VerifyGet(m => m.CommandType, Times.Once);
        }

        [Fact]
        public void ShouldGetAndSetDesignTimeVisible()
        {
            var mock = new Mock<DbCommand>();
            mock.SetupAllProperties();
            var instrumentinstrumentedDbCommand = CreateInstrumentedDbCommand(mock.Object);
            instrumentinstrumentedDbCommand.DesignTimeVisible = true;
            instrumentinstrumentedDbCommand.DesignTimeVisible.Should().BeTrue();
            mock.VerifySet(m => m.DesignTimeVisible = true, Times.Once);
            mock.VerifyGet(m => m.DesignTimeVisible, Times.Once);
        }

        [Fact]
        public void ShouldGetAndSetUpdatedRowSource()
        {
            var mock = new Mock<DbCommand>();
            mock.SetupAllProperties();
            var instrumentinstrumentedDbCommand = CreateInstrumentedDbCommand(mock.Object);
            instrumentinstrumentedDbCommand.UpdatedRowSource = UpdateRowSource.FirstReturnedRecord;
            instrumentinstrumentedDbCommand.UpdatedRowSource.Should().Be(UpdateRowSource.FirstReturnedRecord);
            mock.VerifySet(m => m.UpdatedRowSource = UpdateRowSource.FirstReturnedRecord, Times.Once);
            mock.VerifyGet(m => m.UpdatedRowSource, Times.Once);
        }

        [Fact]
        public void ShouldCallPrepare()
        {
            var mock = new Mock<DbCommand>();
            var instrumentinstrumentedDbCommand = CreateInstrumentedDbCommand(mock.Object);
            instrumentinstrumentedDbCommand.Prepare();
            mock.Verify(m => m.Prepare(), Times.Once);
        }

        [Fact]
        public void ShouldCallCancel()
        {
            var mock = new Mock<DbCommand>();
            var instrumentinstrumentedDbCommand = CreateInstrumentedDbCommand(mock.Object);
            instrumentinstrumentedDbCommand.Cancel();
            mock.Verify(m => m.Cancel(), Times.Once);
        }

        [Fact]
        public void ShouldGetConnectionAsInstrumentedDbConnection()
        {
            var options = new InstrumentationOptions("sqlite");
            var commandMock = new Mock<DbCommand>();
            commandMock.SetupAllProperties();
            var connectionMock = new Mock<DbConnection>();
            var instrumentedDbConnection = new InstrumentedDbConnection(connectionMock.Object, options);
            var command = new InstrumentedDbCommand(commandMock.Object, instrumentedDbConnection, options);
            command.Connection.Should().BeSameAs(instrumentedDbConnection);
            ((InstrumentedDbConnection)command.Connection).InnerDbConnection.Should().BeSameAs(connectionMock.Object);
        }

        [Fact]
        public void ShouldGetConnectionAsNonInstrumentedDbConnection()
        {
            var options = new InstrumentationOptions("sqlite");
            var commandMock = new Mock<DbCommand>();
            commandMock.SetupAllProperties();
            var connectionMock = new Mock<DbConnection>();
            var command = new InstrumentedDbCommand(commandMock.Object, connectionMock.Object, options);
            command.Connection.Should().BeSameAs(connectionMock.Object);
        }

        [Fact]
        public void ShouldSetConnectionAsInstrumentedDbConnection()
        {
            var options = new InstrumentationOptions("sqlite");
            var commandMock = new Mock<DbCommand>();
            commandMock.SetupAllProperties();
            var connectionMock = new Mock<DbConnection>();
            var instrumentedDbConnection = new InstrumentedDbConnection(connectionMock.Object, options);
            var command = new InstrumentedDbCommand(commandMock.Object, instrumentedDbConnection, options);
            command.Connection = instrumentedDbConnection;
            command.Connection.Should().BeSameAs(instrumentedDbConnection);
        }

        [Fact]
        public void ShouldSetConnectionAsNonInstrumentedDbConnection()
        {
            var options = new InstrumentationOptions("sqlite");
            var commandMock = new Mock<DbCommand>();
            commandMock.SetupAllProperties();
            var connectionMock = new Mock<DbConnection>();
            var command = new InstrumentedDbCommand(commandMock.Object, connectionMock.Object, options);
            command.Connection = connectionMock.Object;
            command.Connection.Should().BeSameAs(connectionMock.Object);
        }

        // [Fact]
        // public void ShouldGetTransactionAsInstrumentedDbTransaction()
        // {
        //     var options = new InstrumentationOptions("sqlite");
        //     var commandMock = new Mock<DbCommand>();
        //     commandMock.SetupAllProperties();
        //     var transactionMock = new Mock<DbTransaction>();
        //     var instrumentedDbConnection = new InstrumentedDbConnection(transactionMock.Object, options);
        //     var command = new InstrumentedDbCommand(commandMock.Object, instrumentedDbConnection, options);
        //     command.Connection.Should().BeSameAs(instrumentedDbConnection);
        //     ((InstrumentedDbConnection)command.Connection).InnerDbConnection.Should().BeSameAs(transactionMock.Object);
        // }


        private static InstrumentedDbCommand CreateInstrumentedDbCommand(DbCommand reader)
           => new InstrumentedDbCommand(reader, new Mock<DbConnection>().Object, new InstrumentationOptions("sqlite") { ActivitySource = null });
    }

}