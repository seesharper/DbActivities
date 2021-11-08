using System;
using System.Data.Common;
using Moq;
using Xunit;

namespace DbActivities.Tests
{
    public class DbDataReaderTests
    {
        [Fact]
        public void ShouldCallGetString()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = new InstrumentedDbDataReader(mock.Object, new InstrumentationOptions("sqlite"));
            instrumentedDbDataReader.GetString(0);
            mock.Verify(r => r.GetString(0), Times.Once);
        }

        [Fact]
        public void ShouldCallGetInt16()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = new InstrumentedDbDataReader(mock.Object, new InstrumentationOptions("sqlite"));
            instrumentedDbDataReader.GetInt16(0);
            mock.Verify(r => r.GetInt16(0), Times.Once);
        }

        [Fact]
        public void ShouldCallGetInt32()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = new InstrumentedDbDataReader(mock.Object, new InstrumentationOptions("sqlite"));
            instrumentedDbDataReader.GetInt32(0);
            mock.Verify(r => r.GetInt32(0), Times.Once);
        }

        [Fact]
        public void ShouldCallGetInt64()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = new InstrumentedDbDataReader(mock.Object, new InstrumentationOptions("sqlite"));
            instrumentedDbDataReader.GetInt64(0);
            mock.Verify(r => r.GetInt64(0), Times.Once);
        }

        [Fact]
        public void ShouldCallGetByte()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = new InstrumentedDbDataReader(mock.Object, new InstrumentationOptions("sqlite"));
            instrumentedDbDataReader.GetByte(0);
            mock.Verify(r => r.GetByte(0), Times.Once);
        }

        [Fact]
        public void ShouldCallGetBytes()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = new InstrumentedDbDataReader(mock.Object, new InstrumentationOptions("sqlite"));
            instrumentedDbDataReader.GetBytes(0, 0, Array.Empty<byte>(), 0, 0);
            mock.Verify(r => r.GetBytes(0, 0, Array.Empty<byte>(), 0, 0), Times.Once);
        }

        [Fact]
        public void ShouldCallIndexer()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = new InstrumentedDbDataReader(mock.Object, new InstrumentationOptions("sqlite"));
            var value = instrumentedDbDataReader[0];
            mock.Verify(r => r[0], Times.Once);
        }

        [Fact]
        public void ShouldCallNamedIndexer()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = new InstrumentedDbDataReader(mock.Object, new InstrumentationOptions("sqlite"));
            var value = instrumentedDbDataReader["someColumn"];
            mock.Verify(r => r["someColumn"], Times.Once);
        }

    }
}