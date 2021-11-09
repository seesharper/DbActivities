using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace DbActivities.Tests
{
    public class DbDataReaderTests
    {
        [Fact]
        public void ShouldGetInnerDbDataReader()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.InnerDbDataReader.Should().BeSameAs(mock.Object);
        }

        [Fact]
        public void ShouldCallGetString()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.GetString(0);
            mock.Verify(r => r.GetString(0), Times.Once);
        }

        [Fact]
        public void ShouldCallIndexer()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            var value = instrumentedDbDataReader[0];
            mock.Verify(r => r[0], Times.Once);
        }

        [Fact]
        public void ShouldCallNamedIndexer()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            var value = instrumentedDbDataReader["someColumn"];
            mock.Verify(r => r["someColumn"], Times.Once);
        }

        [Fact]
        public void ShouldCallDepth()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            var value = instrumentedDbDataReader.Depth;
            mock.Verify(r => r.Depth, Times.Once);
        }

        [Fact]
        public void ShouldCallFieldCount()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            var value = instrumentedDbDataReader.FieldCount;
            mock.Verify(r => r.FieldCount, Times.Once);
        }

        [Fact]
        public void ShouldCallHasRows()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            var value = instrumentedDbDataReader.HasRows;
            mock.Verify(r => r.HasRows, Times.Once);
        }

        [Fact]
        public void ShouldCallIsClosed()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            var value = instrumentedDbDataReader.IsClosed;
            mock.Verify(r => r.IsClosed, Times.Once);
        }

        [Fact]
        public void ShouldCallRowsAffected()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            var value = instrumentedDbDataReader.RecordsAffected;
            mock.Verify(r => r.RecordsAffected, Times.Once);
        }

        [Fact]
        public void ShouldCallGetBoolean()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.GetBoolean(0);
            mock.Verify(r => r.GetBoolean(0), Times.Once);
        }

        [Fact]
        public void ShouldCallGetByte()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.GetByte(0);
            mock.Verify(r => r.GetByte(0), Times.Once);
        }

        [Fact]
        public void ShouldCallGetBytes()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.GetBytes(0, 0, Array.Empty<byte>(), 0, 0);
            mock.Verify(r => r.GetBytes(0, 0, Array.Empty<byte>(), 0, 0), Times.Once);
        }

        [Fact]
        public void ShouldCallGetChar()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.GetChar(0);
            mock.Verify(r => r.GetChar(0), Times.Once);
        }

        [Fact]
        public void ShouldCallGetChars()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.GetChars(0, 0, Array.Empty<char>(), 0, 0);
            mock.Verify(r => r.GetChars(0, 0, Array.Empty<char>(), 0, 0), Times.Once);
        }

        [Fact]
        public void ShouldCallGetDataTypeName()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.GetDataTypeName(0);
            mock.Verify(r => r.GetDataTypeName(0), Times.Once);
        }

        [Fact]
        public void ShouldCallGetDataTime()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.GetDateTime(0);
            mock.Verify(r => r.GetDateTime(0), Times.Once);
        }

        [Fact]
        public void ShouldCallGetDecimal()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.GetDecimal(0);
            mock.Verify(r => r.GetDecimal(0), Times.Once);
        }

        [Fact]
        public void ShouldCallGetDouble()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.GetDouble(0);
            mock.Verify(r => r.GetDouble(0), Times.Once);
        }

        [Fact]
        public void ShouldCallGetEnumerator()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.GetEnumerator();
            mock.Verify(e => e.GetEnumerator(), Times.Once);
        }

        [Fact]
        public void ShouldCallGetFieldType()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.GetFieldType(0);
            mock.Verify(e => e.GetFieldType(0), Times.Once);
        }

        [Fact]
        public void ShouldCallGetFieldValue()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.GetFieldValue<int>(0);
            mock.Verify(e => e.GetFieldValue<int>(0), Times.Once);
        }

        [Fact]
        public async Task ShouldCallGetFieldValueAsync()
        {
            var mock = new Mock<DbDataReader>();
            mock.Setup(m => m.GetFieldValueAsync<int>(0, default)).ReturnsAsync(42);
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            var value = await instrumentedDbDataReader.GetFieldValueAsync<int>(0);
            value.Should().Be(42);
        }

        [Fact]
        public void ShouldCallGetFloat()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.GetFloat(0);
            mock.Verify(e => e.GetFloat(0), Times.Once);
        }

        [Fact]
        public void ShouldCallGetGuid()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.GetGuid(0);
            mock.Verify(e => e.GetGuid(0), Times.Once);
        }

        [Fact]
        public void ShouldCallGetInt16()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.GetInt16(0);
            mock.Verify(r => r.GetInt16(0), Times.Once);
        }

        [Fact]
        public void ShouldCallGetInt32()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.GetInt32(0);
            mock.Verify(r => r.GetInt32(0), Times.Once);
        }

        [Fact]
        public void ShouldCallGetInt64()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.GetInt64(0);
            mock.Verify(r => r.GetInt64(0), Times.Once);
        }

        [Fact]
        public void ShouldCallGetName()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.GetName(0);
            mock.Verify(r => r.GetName(0), Times.Once);
        }

        [Fact]
        public void ShouldCallOrdinal()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.GetOrdinal("someColumn");
            mock.Verify(r => r.GetOrdinal("someColumn"), Times.Once);
        }

        [Fact]
        public void ShouldCallGetSchemaTable()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.GetSchemaTable();
            mock.Verify(r => r.GetSchemaTable(), Times.Once);
        }

        [Fact]
        public async Task ShouldCallGetSchemaTableAsync()
        {
            var mock = new Mock<DbDataReader>();
            mock.Setup(m => m.GetSchemaTableAsync(default)).ReturnsAsync(new DataTable());
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            var table = instrumentedDbDataReader.GetSchemaTableAsync();
            table.Should().NotBeNull();
        }

        [Fact]
        public void ShouldCallNextResult()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.NextResult();
            mock.Verify(r => r.NextResult(), Times.Once);
        }

        [Fact]
        public async Task ShouldCallNextResultAsync()
        {
            var mock = new Mock<DbDataReader>();
            mock.Setup(m => m.NextResultAsync(default)).ReturnsAsync(true);
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            var result = await instrumentedDbDataReader.NextResultAsync(default);
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldCallGetStream()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.GetStream(0);
            mock.Verify(r => r.GetStream(0), Times.Once);
        }

        [Fact]
        public void ShouldCallGetTextReader()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.GetTextReader(0);
            mock.Verify(r => r.GetTextReader(0), Times.Once);
        }

        [Fact]
        public void ShouldCallGetValue()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.GetValue(0);
            mock.Verify(r => r.GetValue(0), Times.Once);
        }

        [Fact]
        public void ShouldCallGetValues()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.GetValues(Array.Empty<object>());
            mock.Verify(r => r.GetValues(Array.Empty<object>()), Times.Once);
        }

        [Fact]
        public void ShouldCallIsDbNull()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.IsDBNull(0);
            mock.Verify(r => r.IsDBNull(0), Times.Once);
        }

        [Fact]
        public void ShouldCallClose()
        {
            var mock = new Mock<DbDataReader>();
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            instrumentedDbDataReader.Close();
            mock.Verify(r => r.Close(), Times.Once);
        }

        [Fact]
        public async Task ShouldCallIsDbNullAsync()
        {
            var mock = new Mock<DbDataReader>();
            mock.Setup(m => m.IsDBNullAsync(0, default)).ReturnsAsync(true);
            var instrumentedDbDataReader = CreateInstrumentedDbDataReader(mock.Object);
            var result = await instrumentedDbDataReader.IsDBNullAsync(0, default);
            result.Should().BeTrue();
        }

        private static InstrumentedDbDataReader CreateInstrumentedDbDataReader(DbDataReader reader)
            => new InstrumentedDbDataReader(reader, new InstrumentationOptions("sqlite") { ActivitySource = null });
    }
}