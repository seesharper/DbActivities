using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics.Testing;
using Moq;
using Moq.Protected;
using Xunit;

namespace DbActivities.Tests;

[Collection("ActivityTests")]
public class MetricsTests
{
    [Fact]
    public void ShouldAddOpenedConnectionsMetric()
    {
        var collector = new MetricCollector<int>(InstrumentationOptions.Meter, "db.connections.open");

        var mock = new Mock<DbConnection>();
        var options = new InstrumentationOptions();
        var instrumentedDbConnection = new InstrumentedDbConnection(mock.Object, options);
        instrumentedDbConnection.Dispose();
        var measurements = collector.GetMeasurementSnapshot();
        measurements.Select(m => m.Value).Should().Contain(1);
        measurements.Select(m => m.Value).Should().Contain(-1);
    }

    [Fact]
    public async Task ShouldAddConnectionsUseTimeMetric()
    {
        var collector = new MetricCollector<long>(InstrumentationOptions.Meter, "db.client.connections.use_time");

        // Act
        var mock = new Mock<DbConnection>();
        var options = new InstrumentationOptions();
        var instrumentedDbConnection = new InstrumentedDbConnection(mock.Object, options);
        await Task.Delay(100);
        instrumentedDbConnection.Dispose();

        var measurements = collector.GetMeasurementSnapshot();
        measurements.Should().HaveCount(1);
        Assert.Equal(1, measurements.Count);
    }

    [Fact]
    public async Task ShouldAddTransactionsUseTimeMetric()
    {
        var collector = new MetricCollector<long>(InstrumentationOptions.Meter, "db.client.transactions.use_time");

        var connection = GetConnection();
        using (var transaction = connection.BeginTransaction())
        {
            await Task.Delay(100);
            transaction.Commit();
        }

        var measurements = collector.GetMeasurementSnapshot();
        measurements.Should().HaveCount(1);
        Assert.Equal(1, measurements.Count);
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
}