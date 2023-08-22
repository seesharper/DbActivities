namespace DbActivities.Tests;

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Language.Flow;
using Moq.Protected;

public static class MockExtensions
{
    public static ISetup<DbCommand, DbDataReader> SetupDbDataReader(this Mock<DbCommand> commandMock)
    {
        return commandMock.Protected().Setup<DbDataReader>("ExecuteDbDataReader", ItExpr.IsAny<CommandBehavior>());
    }

    public static ISetup<DbCommand, Task<DbDataReader>> SetupDbDataReaderAsync(this Mock<DbCommand> commandMock)
    {
        var test = commandMock.Protected().Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>());
        return test;
    }

    public static ISetup<DbCommand, int> SetupExecuteNonQuery(this Mock<DbCommand> commandMock)
    {
        return commandMock.Setup(command => command.ExecuteNonQuery());
    }

    public static ISetup<DbCommand, Task<int>> SetupExecuteNonQueryAsync(this Mock<DbCommand> commandMock)
    {
        return commandMock.Setup(command => command.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()));
    }

    public static ISetup<DbCommand, object> SetupExecuteScalar(this Mock<DbCommand> commandMock)
    {
        return commandMock.Setup(command => command.ExecuteScalar());
    }

    public static ISetup<DbCommand, Task<object>> SetupExecuteScalarAsync(this Mock<DbCommand> commandMock)
    {
        return commandMock.Setup(command => command.ExecuteScalarAsync(It.IsAny<CancellationToken>()));
    }
}


public static class DataReaderExtensions
{
    public static void ReadToEnd(this DbDataReader reader)
    {
        while (reader.Read())
        {
        }
        ((IDisposable)reader).Dispose();
    }

    public static async Task ReadToEndAsync(this DbDataReader reader)
    {
        while (await reader.ReadAsync())
        {
        }
        await reader.DisposeAsync();
    }
}