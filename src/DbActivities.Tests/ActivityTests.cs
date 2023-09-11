using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;

namespace DbActivities.Tests
{

    [Collection("ActivityTests")]
    public class ActivityTests : IDisposable
    {
        private readonly List<Activity> _startedActivities = new();

        private ActivityListener _activityListener;

        public ActivityTests()
        {
            _activityListener = new ActivityListener
            {
                ShouldListenTo = (source) => true,
                Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllDataAndRecorded,
                ActivityStarted = activity =>
                {
                    _startedActivities.Add(activity);
                }
            };

            ActivitySource.AddActivityListener(_activityListener);

        }

        public void Dispose()
        {
            _activityListener.ShouldListenTo = (source) => false;
            _activityListener.ActivityStarted = activity => { };
            _activityListener.Dispose();
        }

        [Fact]
        public void ShouldPopulateConnectionLevelTags()
        {
            using (var connection = GetConnection())
            {
            }
            var connectionActivity = _startedActivities.GetActivity(nameof(InstrumentedDbConnection));
            connectionActivity.ShouldHaveConnectionLevelTags();
        }

        [Fact]
        public void ShouldHandleConnectionBeingDisposedTwice()
        {
            var connection = GetConnection();
            connection.Dispose();
            connection.Dispose();

            _startedActivities.Count.Should().Be(1);
        }

        [Fact]
        public async Task ShouldHandleConnectionBeingDisposedAsyncTwice()
        {
            var connection = GetConnection();
            await connection.DisposeAsync();
            await connection.DisposeAsync();

            _startedActivities.Count.Should().Be(1);
        }

        [Fact]
        public void ShouldHandleConnectionActivityBeingNullWhenDisposed()
        {
            var options = new InstrumentationOptions();
            options.ConfigureActivityStarter((source, kind) => null);
            using (var connection = GetConnection(options: options))
            {
            }
            VerifyNoActivities();
        }

        [Fact]
        public async Task ShouldHandleConnectionActivityBeingNullWhenDisposedAsync()
        {
            var options = new InstrumentationOptions();
            options.ConfigureActivityStarter((source, kind) => null);
            await using (var connection = GetConnection(options: options))
            {
            }
            VerifyNoActivities();
        }

        [Fact]
        public async Task ShouldHandleActivityBeingNullWhenExecuteNonQueryAsync()
        {
            var options = new InstrumentationOptions();
            options.ConfigureActivityStarter((source, kind) => null);
            var command = GetCommand(m => m.SetupExecuteNonQuery().Returns(1), options);
            await command.ExecuteNonQueryAsync();
            VerifyNoActivities();
        }


        [Fact]
        public void ShouldPopulateCallLevelTagsWhenExecutingNonQuery()
        {
            GetConnection().CreateCommand().ExecuteNonQuery();
            VerifyCommandActivity(nameof(InstrumentedDbCommand.ExecuteNonQuery), activity => activity.ShouldHaveCallLevelTags(OperationType.NonQuery));
        }

        [Fact]
        public async Task ShouldPopulateCallLevelTagsWhenExecutingNonQueryAsync()
        {
            await GetConnection().CreateCommand().ExecuteNonQueryAsync();
            VerifyCommandActivity(nameof(InstrumentedDbCommand.ExecuteNonQueryAsync), activity => activity.ShouldHaveCallLevelTags(OperationType.NonQuery));
        }

        [Fact]
        public void ShouldAddExceptionEventWhenExecutingNonQuery()
        {
            try
            {
                var command = GetCommand(cm => cm.SetupExecuteNonQuery().Throws<Exception>());
                command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                VerifyCommandActivity(nameof(InstrumentedDbCommand.ExecuteNonQuery), activity => activity.ShouldHaveExceptionEvent());
            }
        }

        [Fact]
        public async Task ShouldAddExceptionEventWhenExecutingNonQueryAsync()
        {
            try
            {
                var command = GetCommand(cm => cm.SetupExecuteNonQueryAsync().Throws<Exception>());
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception)
            {
                VerifyCommandActivity(nameof(InstrumentedDbCommand.ExecuteNonQueryAsync), activity => activity.ShouldHaveExceptionEvent());
            }
        }

        [Fact]
        public void ShouldPopulateCallLevelTagsWhenExecutingScalar()
        {
            GetCommand(cm => cm.SetupExecuteScalar().Returns(1)).ExecuteScalar();
            VerifyCommandActivity(nameof(InstrumentedDbCommand.ExecuteScalar), activity => activity.ShouldHaveCallLevelTags(OperationType.Scalar));
        }

        [Fact]
        public async Task ShouldPopulateCallLevelTagsWhenExecutingScalarAsync()
        {
            await GetCommand(cm => cm.SetupExecuteScalarAsync().ReturnsAsync(1)).ExecuteScalarAsync();
            VerifyCommandActivity(nameof(InstrumentedDbCommand.ExecuteScalarAsync), activity => activity.ShouldHaveCallLevelTags(OperationType.Scalar));
        }

        [Fact]
        public void ShouldAddExceptionEventWhenExecutingScalar()
        {
            try
            {
                var command = GetCommand(cm => cm.SetupExecuteScalar().Throws<Exception>());
                command.ExecuteScalar();
            }
            catch (Exception)
            {
                VerifyCommandActivity(nameof(InstrumentedDbCommand.ExecuteScalar), activity => activity.ShouldHaveExceptionEvent());
            }
        }

        [Fact]
        public async Task ShouldAddExceptionEventWhenExecutingScalarAsync()
        {
            try
            {
                var command = GetCommand(cm => cm.SetupExecuteScalarAsync().Throws<Exception>());
                await command.ExecuteScalarAsync();
            }
            catch (Exception)
            {
                VerifyCommandActivity(nameof(InstrumentedDbCommand.ExecuteScalarAsync), activity => activity.ShouldHaveExceptionEvent());
            }
        }

        [Fact]
        public void ShouldPopulateCallLevelTagsWhenExecutingReader()
        {
            GetCommand().ExecuteReader();
            VerifyCommandActivity(nameof(InstrumentedDbCommand.ExecuteReader), activity => activity.ShouldHaveCallLevelTags(OperationType.Reader));
        }

        [Fact]
        public async Task ShouldPopulateCallLevelTagsWhenExecutingReaderAsync()
        {
            await GetCommand().ExecuteReaderAsync();
            VerifyCommandActivity(nameof(InstrumentedDbCommand.ExecuteReaderAsync), activity => activity.ShouldHaveCallLevelTags(OperationType.Reader));
        }

        [Fact]
        public void ShouldAddExceptionEventWhenExecutingReader()
        {
            try
            {
                GetCommand(cm => cm.SetupDbDataReader().Throws<Exception>()).ExecuteReader();
            }
            catch (Exception)
            {
                VerifyCommandActivity(nameof(InstrumentedDbCommand.ExecuteReader), activity => activity.ShouldHaveExceptionEvent());
            }
        }

        [Fact]
        public async Task ShouldAddExceptionEventWhenExecutingReaderAsync()
        {
            try
            {
                await GetCommand(cm => cm.SetupDbDataReaderAsync().Throws<Exception>()).ExecuteReaderAsync();
            }
            catch (Exception)
            {
                VerifyCommandActivity(nameof(InstrumentedDbCommand.ExecuteReaderAsync), activity => activity.ShouldHaveExceptionEvent());
            }
        }

        [Fact]
        public void ShouldAddRowsReadWhenExecutingReader()
        {
            GetConnection().CreateCommand().ExecuteReader().ReadToEnd();
            VerifyReaderActivity(activity => activity.Tags.Should().Contain(tag => tag.Key == CustomTagNames.RowsRead && tag.Value == "3"));
        }

        [Fact]
        public async Task ShouldAddRowsReadWhenExecutingReaderAsync()
        {
            await (await GetConnection().CreateCommand().ExecuteReaderAsync()).ReadToEndAsync();
            VerifyReaderActivity(activity => activity.Tags.Should().Contain(tag => tag.Key == CustomTagNames.RowsRead && tag.Value == "3"));
        }

        [Fact]
        public void ShouldCreateTransactionWithConnectionActivityAsParent()
        {
            GetConnection().BeginTransaction();
            GetTransactionActivity().Parent.Should().Be(GetConnectionActivity());
        }

        [Fact]
        public async Task ShouldCreateTransactionWithConnectionActivityAsParentAsync()
        {
            await GetConnection().BeginTransactionAsync();
            GetTransactionActivity().Parent.Should().Be(GetConnectionActivity());
        }

        [Fact]
        public void ShouldCreateTransactionWithIsolationLevelWithConnectionActivityAsParent()
        {
            GetConnection().BeginTransaction(IsolationLevel.ReadUncommitted);
            GetTransactionActivity().Parent.Should().Be(GetConnectionActivity());
        }

        [Fact]
        public void ShouldAddCommitEventWhenTransactionIsCommitted()
        {
            GetConnection().BeginTransaction().Commit();
            VerifyTransactionEvent(nameof(InstrumentedDbTransaction.Commit));
        }

        [Fact]
        public async Task ShouldAddCommitAsyncEventWhenTransactionIsCommittedAsync()
        {
            await GetConnection().BeginTransaction().CommitAsync();
            VerifyTransactionEvent(nameof(InstrumentedDbTransaction.CommitAsync));
        }

        [Fact]
        public void ShouldAddRollbackEventWhenTransactionIsRolledBack()
        {
            GetConnection().BeginTransaction().Rollback();
            VerifyTransactionEvent(nameof(InstrumentedDbTransaction.Rollback));
        }

        [Fact]
        public async Task ShouldAddRollbackAsyncEventWhenTransactionIsRolledBackAsync()
        {
            await GetConnection().BeginTransaction().RollbackAsync();
            VerifyTransactionEvent(nameof(InstrumentedDbTransaction.RollbackAsync));
        }

        [Fact]
        public async Task ShouldHandleTransactionWhenActivityIsNull()
        {
            var options = new InstrumentationOptions();
            options.ConfigureActivityStarter((source, kind) => null);
            GetConnection(options: options).BeginTransaction().Commit();
            GetConnection(options: options).BeginTransaction().Rollback();
            await GetConnection(options: options).BeginTransaction().CommitAsync();
            await GetConnection(options: options).BeginTransaction().RollbackAsync();

            VerifyNoActivities();
        }

        [Fact]
        public void ShouldSetAndGetTransactionFromDbCommand()
        {
            using (var connection = GetConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    command.Transaction = transaction;
                    command.Transaction.Should().BeOfType<InstrumentedDbTransaction>();
                    command.Transaction = ((InstrumentedDbTransaction)transaction).InnerDbTransaction;
                    command.Transaction.Should().BeOfType(((InstrumentedDbTransaction)transaction).InnerDbTransaction.GetType());
                }
            }
        }

        [Fact]
        public async Task ShouldHandleExecuteDbDataReaderAsyncWhenActivityIsNull()
        {
            var options = new InstrumentationOptions();
            options.ConfigureActivityStarter((source, kind) => null);
            await GetConnection(options: options).CreateCommand().ExecuteReaderAsync();
            VerifyNoActivities();
        }

        [Fact]
        public async Task ShouldHandleExceptionWhenActivityIsNullForReadAsync()
        {
            var options = new InstrumentationOptions();
            options.ConfigureActivityStarter((source, kind) => null);
            var command = GetCommand(m => m.SetupDbDataReaderAsync().Throws<Exception>(), options);

            try
            {
                var reader = await command.ExecuteReaderAsync();
            }
            catch (Exception)
            {
                VerifyNoActivities();
            }

        }

        [Fact]
        public async Task ShouldHandleExceptionWhenActivityIsNullForExecuteNonQueryAsync()
        {
            var options = new InstrumentationOptions();
            options.ConfigureActivityStarter((source, kind) => null);
            var command = GetCommand(m => m.SetupExecuteNonQueryAsync().Throws<Exception>(), options);

            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception)
            {
                _startedActivities.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task ShouldHandleExceptionWhenActivityIsNullForReadScalarAsync()
        {
            var options = new InstrumentationOptions();
            options.ConfigureActivityStarter((source, kind) => null);
            var command = GetCommand(m => m.SetupExecuteScalarAsync().Throws<Exception>(), options);

            try
            {
                await command.ExecuteScalarAsync();
            }
            catch (Exception)
            {
                _startedActivities.Should().BeEmpty();
            }
        }

        [Fact]
        public void ShouldCallFormatCommandText()
        {
            var options = new InstrumentationOptions().FormatCommandText<DbCommand>(command => "MyCommandText");
            var command = GetCommand(m => m.SetupExecuteNonQuery().Returns(1), options);
            command.ExecuteNonQuery();

            VerifyCommandActivity(nameof(InstrumentedDbCommand.ExecuteNonQuery), activity => activity.Tags.Should().Contain(tag => tag.Key == "db.statement" && tag.Value == "MyCommandText"));
        }

        [Fact]
        public void ShouldCallConfigureActivity()
        {
            var options = new InstrumentationOptions().ConfigureActivity(a => a.DisplayName = "MyDisplayName");
            var command = GetCommand(m => m.SetupExecuteNonQuery().Returns(1), options);
            command.ExecuteNonQuery();
            VerifyCommandActivity(nameof(InstrumentedDbCommand.ExecuteNonQuery), activity => activity.DisplayName.Should().Be("MyDisplayName"));
        }

        [Fact]
        public void ShouldSetUserTag()
        {
            var options = new InstrumentationOptions
            {
                User = "TestUser"
            };
            var command = GetCommand(m => m.SetupExecuteNonQuery().Returns(1), options);
            command.ExecuteNonQuery();
            VerifyCommandActivity(nameof(InstrumentedDbCommand.ExecuteNonQuery), activity => activity.Tags.Should().Contain(tag => tag.Key == "db.user" && tag.Value == "TestUser"));
        }


        [Fact]
        public void ShouldConfigureCommandActivity()
        {
            var options = new InstrumentationOptions();
            var wasConfigured = false;
            options.ConfigureCommandActivity<DbCommand>((activity, command) =>
            {
                wasConfigured = true;
            });

            var command = GetCommand(m => m.SetupExecuteNonQuery().Returns(1), options);
            command.ExecuteNonQuery();

            wasConfigured.Should().BeTrue();
        }

        [Fact]
        public void ShouldConfigureDataReaderActivity()
        {
            var options = new InstrumentationOptions();
            bool wasConfigured = false;
            options.ConfigureDataReaderActivity<DbDataReader>((activity, command) =>
            {
                wasConfigured = true;
            });

            var command = GetCommand(options: options);
            command.ExecuteReader().ReadToEnd();

            wasConfigured.Should().BeTrue();
        }

        [Fact]
        public void ShouldConfigureConnectionActivity()
        {
            var options = new InstrumentationOptions();
            bool wasConfigured = false;
            options.ConfigureConnectionActivity<DbConnection>((activity, connection) =>
            {
                wasConfigured = true;
            });

            using (var connection = GetConnection(options: options))
            {
            }

            wasConfigured.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldConfigureConnectionActivityAsync()
        {
            var options = new InstrumentationOptions();
            bool wasConfigured = false;
            options.ConfigureConnectionActivity<DbConnection>((activity, connection) =>
            {
                wasConfigured = true;
            });

            await using (var connection = GetConnection(options: options))
            {
            }

            wasConfigured.Should().BeTrue();
        }

        [Fact]
        public void ShouldConfigureTransactionActivity()
        {
            var options = new InstrumentationOptions();
            bool wasConfigured = false;
            options.ConfigureTransactionActivity<DbTransaction>((activity, transaction) =>
            {
                wasConfigured = true;
            });

            using (var connection = GetConnection(options: options))
            {
                using (var transaction = connection.BeginTransaction())
                {

                }
            }

            wasConfigured.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldConfigureTransactionActivityAsync()
        {
            var options = new InstrumentationOptions();
            bool wasConfigured = false;
            options.ConfigureTransactionActivity<DbTransaction>((activity, transaction) =>
            {
                wasConfigured = true;
            });
            var connection = GetConnection(options: options);

            using (var transaction = await connection.BeginTransactionAsync())
            {
                await transaction.DisposeAsync();
            }

            wasConfigured.Should().BeTrue();
        }

        [Fact]
        public void ShouldConfigureDbCommand()
        {
            bool wasConfigured = false;
            var options = new InstrumentationOptions("sqlite");
            options.ConfigureDbCommand<DbCommand>(command =>
            {
                wasConfigured = true;
            });

            GetConnection(options: options).CreateCommand().ExecuteReader().ReadToEnd();
            wasConfigured.Should().BeTrue();
        }

        [Fact]
        public void ShouldUseCustomCommandFactory()
        {
            var options = new InstrumentationOptions("sqlite");
            options.ConfigureCommandFactory((command, connection, options) => new CustomInstrumentedDbCommand(command, connection, options));
            using (var connection = GetConnection(options: options))
            {
                connection.CreateCommand().Should().BeOfType<CustomInstrumentedDbCommand>();
            }
        }

        private static DbConnection GetConnection(Action<Mock<DbCommand>> configureCommandMock = null, InstrumentationOptions options = null)
        {
            configureCommandMock ??= (mock) => { };
            options ??= new InstrumentationOptions("sqlite");
            var connectionMock = new Mock<DbConnection>();
            var commandMock = new Mock<DbCommand>();

            var dataReaderMock = new Mock<DbDataReader>();
            dataReaderMock.SetupSequence(r => r.Read()).Returns(true).Returns(true).Returns(true).Returns(false);
            dataReaderMock.SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(true).ReturnsAsync(true).ReturnsAsync(false);

            var transactionMock = new Mock<DbTransaction>();
            connectionMock.Protected().Setup<DbTransaction>("BeginDbTransaction", ItExpr.IsAny<IsolationLevel>()).Returns(transactionMock.Object);
            connectionMock.Protected().Setup<ValueTask<DbTransaction>>("BeginDbTransactionAsync", ItExpr.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()).Returns(ValueTask.FromResult(transactionMock.Object));
            connectionMock.SetupProperty(c => c.ConnectionString, "SomeConnectionString");

            //connectionMock.Protected().Setup<DbTransaction>("BeginDbTransaction").Returns(transactionMock.Object);

            commandMock.SetupGet(c => c.CommandText).Returns("SampleCommandText");

            connectionMock.Protected().Setup<DbCommand>("CreateDbCommand").Returns(commandMock.Object);



            commandMock.Protected().Setup<DbDataReader>("ExecuteDbDataReader", It.IsAny<CommandBehavior>()).Returns(dataReaderMock.Object);
            commandMock.Protected().Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()).Returns(Task.FromResult(dataReaderMock.Object));
            var connection = new InstrumentedDbConnection(connectionMock.Object, options);
            configureCommandMock(commandMock);
            return connection;
        }

        private static DbCommand GetCommand(Action<Mock<DbCommand>> configureCommandMock = null, InstrumentationOptions options = null)
        {
            return GetConnection(configureCommandMock, options).CreateCommand();
        }

        private void VerifyActivity(string name, Action<Activity> verify)
        {
            var activity = _startedActivities.GetActivity(name);
            verify(activity);
        }

        private void VerifyCommandActivity(string name, Action<Activity> verify)
        {
            var activity = _startedActivities.GetActivity($"{nameof(InstrumentedDbCommand)}.{name}");
            verify(activity);
        }

        private void VerifyReaderActivity(Action<Activity> verify)
        {
            var activity = _startedActivities.GetActivity(nameof(InstrumentedDbDataReader));
            verify(activity);
        }

        private Activity GetConnectionActivity()
        {
            return _startedActivities.GetActivity(nameof(InstrumentedDbConnection));
        }

        private void VerifyTransactionEvent(string eventName)
        {
            var transactionActivity = _startedActivities.GetActivity(nameof(InstrumentedDbTransaction));
            transactionActivity.Events.Should().Contain(e => e.Name == eventName);
        }

        private void VerifyNoActivities()
        {
            _startedActivities.Should().BeEmpty();
        }

        private Activity GetTransactionActivity()
        {
            return _startedActivities.GetActivity(nameof(InstrumentedDbTransaction));
        }

    }
}
