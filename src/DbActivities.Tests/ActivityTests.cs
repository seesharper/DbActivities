using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DbReader;
using FluentAssertions;
using Xunit;

namespace DbActivities.Tests
{
    public class ActivityTests
    {
        private readonly List<Activity> _startedActivities = new();

        public ActivityTests()
        {
            var activityListener = new ActivityListener
            {
                ShouldListenTo = (source) => true,
                Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllDataAndRecorded,
                ActivityStarted = activity =>
                {
                    _startedActivities.Add(activity);
                }
            };

            ActivitySource.AddActivityListener(activityListener);
        }

        [Fact]
        public async Task ShouldPopulateConnectionLevelTags()
        {
            using (GetConnection())
            {
                await Task.Delay(1);
            }

            var connectionActivity = _startedActivities.GetActivity(nameof(InstrumentedDbConnection));
            connectionActivity.ShouldHaveConnectionLevelTags();
        }

        [Fact]
        public async Task ShouldPopulateCallLevelTagsWhenExecutingNonQuery()
        {
            using (var connection = GetConnection())
            {
                connection.Execute(Sql.CreateTestTable);
                await Task.Delay(1);
            };

            var commandActivity = _startedActivities.GetActivity($"{nameof(InstrumentedDbCommand)}.{nameof(InstrumentedDbCommand.ExecuteNonQuery)}");
            commandActivity.ShouldHaveCallLevelTags(OperationType.NonQuery);
        }

        [Fact]
        public async Task ShouldPopulateCallLevelTagsWhenExecutingNonQueryAsync()
        {
            using (var connection = GetConnection())
            {
                await connection.ExecuteAsync(Sql.CreateTestTable);
                await Task.Delay(1);
            };

            var commandActivity = _startedActivities.GetActivity($"{nameof(InstrumentedDbCommand)}.{nameof(InstrumentedDbCommand.ExecuteNonQueryAsync)}");
            commandActivity.ShouldHaveCallLevelTags(OperationType.NonQuery);
        }

        [Fact]
        public async Task ShouldAddExceptionEventWhenExecutingNonQuery()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Execute(Sql.Rubbish);
                    await Task.Delay(1);
                };
            }
            catch (Exception)
            {
                var commandActivity = _startedActivities.GetActivity($"{nameof(InstrumentedDbCommand)}.{nameof(InstrumentedDbCommand.ExecuteNonQuery)}");
                commandActivity.ShouldHaveExceptionEvent();
            }
        }

        [Fact]
        public async Task ShouldAddExceptionEventWhenExecutingNonQueryAsync()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    await connection.ExecuteAsync(Sql.Rubbish);
                    await Task.Delay(1);
                };
            }
            catch (Exception)
            {
                var commandActivity = _startedActivities.GetActivity($"{nameof(InstrumentedDbCommand)}.{nameof(InstrumentedDbCommand.ExecuteNonQueryAsync)}");
                commandActivity.ShouldHaveExceptionEvent();
            }
        }

        [Fact]
        public async Task ShouldPopulateCallLevelTagsWhenExecutingScalar()
        {
            using (var connection = GetConnection())
            {
                connection.Execute(Sql.CreateTestTable);
                var command = connection.CreateCommand(Sql.GetCount);
                command.ExecuteScalar();
                await Task.Delay(1);
            };

            var commandActivity = _startedActivities.GetActivity($"{nameof(InstrumentedDbCommand)}.{nameof(InstrumentedDbCommand.ExecuteScalar)}");
            commandActivity.ShouldHaveCallLevelTags(OperationType.Scalar);
        }

        [Fact]
        public async Task ShouldPopulateCallLevelTagsWhenExecutingScalarAsync()
        {
            using (var connection = GetConnection())
            {
                connection.Execute(Sql.CreateTestTable);
                var command = (DbCommand)connection.CreateCommand(Sql.GetCount);
                await command.ExecuteScalarAsync();
                await Task.Delay(1);
            };

            var commandActivity = _startedActivities.GetActivity($"{nameof(InstrumentedDbCommand)}.{nameof(InstrumentedDbCommand.ExecuteScalarAsync)}");
            commandActivity.ShouldHaveCallLevelTags(OperationType.Scalar);
        }


        [Fact]
        public async Task ShouldAddExceptionEventWhenExecutingScalar()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Execute(Sql.CreateTestTable);
                    var command = connection.CreateCommand(Sql.GetCount);
                    command.CommandText = Sql.Rubbish;
                    command.ExecuteScalar();
                    await Task.Delay(1);
                };
            }
            catch (Exception)
            {
                var commandActivity = _startedActivities.GetActivity($"{nameof(InstrumentedDbCommand)}.{nameof(InstrumentedDbCommand.ExecuteScalar)}");
                commandActivity.ShouldHaveExceptionEvent();
            }
        }

        [Fact]
        public async Task ShouldAddExceptionEventWhenExecutingScalarAsync()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Execute(Sql.CreateTestTable);
                    var command = (DbCommand)connection.CreateCommand(Sql.GetCount);
                    command.CommandText = Sql.Rubbish;
                    await command.ExecuteScalarAsync();
                    await Task.Delay(1);
                };
            }
            catch (Exception)
            {
                var commandActivity = _startedActivities.GetActivity($"{nameof(InstrumentedDbCommand)}.{nameof(InstrumentedDbCommand.ExecuteScalarAsync)}");
                commandActivity.ShouldHaveExceptionEvent();
            }
        }

        [Fact]
        public async Task ShouldPopulateCallLevelTagsWhenExecutingReader()
        {
            using (var connection = GetConnection())
            {
                connection.Execute(Sql.CreateTestTable);
                connection.Read<TestRecord>("SELECT Id FROM TestTable");
                await Task.Delay(1);
            };

            var commandActivity = _startedActivities.GetActivity($"{nameof(InstrumentedDbCommand)}.{nameof(InstrumentedDbCommand.ExecuteReader)}");
            commandActivity.ShouldHaveCallLevelTags(OperationType.Reader);
        }

        [Fact]
        public async Task ShouldPopulateCallLevelTagsWhenExecutingReaderAsync()
        {
            using (var connection = GetConnection())
            {
                connection.Execute(Sql.CreateTestTable);
                await connection.ReadAsync<TestRecord>("SELECT Id FROM TestTable");
                await Task.Delay(1);
            };

            var commandActivity = _startedActivities.GetActivity($"{nameof(InstrumentedDbCommand)}.{nameof(InstrumentedDbCommand.ExecuteReaderAsync)}");
            commandActivity.ShouldHaveCallLevelTags(OperationType.Reader);
        }

        [Fact]
        public async Task ShouldAddExceptionEventWhenExecutingReader()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Read<TestRecord>(Sql.Rubbish);
                    await Task.Delay(1);
                };
            }
            catch (Exception)
            {
                var commandActivity = _startedActivities.GetActivity($"{nameof(InstrumentedDbCommand)}.{nameof(InstrumentedDbCommand.ExecuteReader)}");
                commandActivity.ShouldHaveExceptionEvent();
            }
        }

        [Fact]
        public async Task ShouldAddExceptionEventWhenExecutingReaderAsync()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    await connection.ReadAsync<TestRecord>(Sql.Rubbish);
                    await Task.Delay(1);
                };
            }
            catch (Exception)
            {
                var commandActivity = _startedActivities.GetActivity($"{nameof(InstrumentedDbCommand)}.{nameof(InstrumentedDbCommand.ExecuteReaderAsync)}");
                commandActivity.ShouldHaveExceptionEvent();
            }
        }


        [Fact]
        public async Task ShouldAddRowsReadWhenExecutingReader()
        {
            using (var connection = GetConnection())
            {
                connection.Execute("CREATE TABLE TestTable (Id int null)");
                connection.Execute("INSERT INTO TestTable(Id) VALUES(@id)", new { Id = 1 });
                connection.Execute("INSERT INTO TestTable(Id) VALUES(@id)", new { Id = 2 });
                connection.Execute("INSERT INTO TestTable(Id) VALUES(@id)", new { Id = 3 });
                var result = connection.Read<TestRecord>("SELECT Id FROM TestTable");
                result.Count().Should().Be(3);
                var readerActivity = _startedActivities.GetActivity(nameof(InstrumentedDbDataReader));
                readerActivity.Tags.Should().Contain(tag => tag.Key == CustomTagNames.RowsRead && tag.Value == "3");
            }
        }

        [Fact]
        public async Task ShouldAddRowsReadWhenExecutingReaderAsync()
        {
            using (var connection = GetConnection())
            {
                connection.Execute("CREATE TABLE TestTable (Id int null)");
                connection.Execute("INSERT INTO TestTable(Id) VALUES(@id)", new { Id = 1 });
                connection.Execute("INSERT INTO TestTable(Id) VALUES(@id)", new { Id = 2 });
                connection.Execute("INSERT INTO TestTable(Id) VALUES(@id)", new { Id = 3 });
                var result = await connection.ReadAsync<TestRecord>("SELECT Id FROM TestTable");
                result.Count().Should().Be(3);
                var readerActivity = _startedActivities.GetActivity(nameof(InstrumentedDbDataReader));
                readerActivity.Tags.Should().Contain(tag => tag.Key == CustomTagNames.RowsRead && tag.Value == "3");
            }
        }

        private DbConnection GetConnection()
        {
            var connection = new SQLiteConnection("Data Source= :memory:; Cache = Shared");
            connection.Open();
            return new InstrumentedDbConnection(connection, new InstrumentationOptions("sqlite"));
        }


    }

    public record TestRecord(int Id);


    public static class Sql
    {
        public static string CreateTestTable { get => "CREATE TABLE TestTable (Id int null)"; }

        public static string Rubbish { get => "Rubbish"; }

        public static string GetCount { get => "SELECT COUNT(*) FROM TestTable"; }
    }
}
