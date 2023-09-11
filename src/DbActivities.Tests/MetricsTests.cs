using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Metrics;
using FluentAssertions;
using Moq;
using Xunit;

namespace DbActivities.Tests;

[Collection("ActivityTests")]
public class MetricsTests
{
    private readonly MeterListener _meterListener;
    private List<int> _measurements = new List<int>();


    public MetricsTests()
    {
        _meterListener = new MeterListener();
        _meterListener.InstrumentPublished = (instrument, meterListener) =>
        {
            meterListener.EnableMeasurementEvents(instrument);
        };
        _meterListener.SetMeasurementEventCallback<int>((Instrument instrument, int measurement, ReadOnlySpan<KeyValuePair<string, object>> tags, object state) =>
        {
            _measurements.Add(measurement);
        });


        _meterListener.Start();


    }

    [Fact]
    public void ShouldAddOpenedConnectionsMetric()
    {
        var mock = new Mock<DbConnection>();
        var options = new InstrumentationOptions();
        var instrumentedDbConnection = new InstrumentedDbConnection(mock.Object, options);
        instrumentedDbConnection.Dispose();
        _measurements.Should().Contain(1);
        _measurements.Should().Contain(-1);
    }
}