using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DbActivities
{
    public class InstrumentedDbDataReader : DbDataReader
    {
        private readonly DbDataReader _innerDbDataReader;
        private readonly InstrumentationOptions _options;

        private readonly DbCommand _command;

        private readonly Activity _readerActivity;

        private int _rowsRead = 0;

        public InstrumentedDbDataReader(DbDataReader dbDataReader, InstrumentationOptions options)
        {
            _innerDbDataReader = dbDataReader;
            _options = options;
            _readerActivity = options.ActivitySource.StartActivity($"{nameof(InstrumentedDbDataReader)}");
        }

        public DbDataReader InnerDbDataReader { get => _innerDbDataReader; }

        public override object this[int ordinal] => _innerDbDataReader[ordinal];

        public override object this[string name] => _innerDbDataReader[name];

        public override int Depth => _innerDbDataReader.Depth;

        public override int FieldCount => _innerDbDataReader.FieldCount;

        public override bool HasRows => _innerDbDataReader.HasRows;

        public override bool IsClosed => _innerDbDataReader.IsClosed;

        public override int RecordsAffected => _innerDbDataReader.RecordsAffected;

        public override int VisibleFieldCount => base.VisibleFieldCount;

        public override bool Equals(object obj) => _innerDbDataReader.Equals(obj);
        public override bool GetBoolean(int ordinal) => _innerDbDataReader.GetBoolean(ordinal);
        public override byte GetByte(int ordinal) => _innerDbDataReader.GetByte(ordinal);
        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) => _innerDbDataReader.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
        public override char GetChar(int ordinal) => _innerDbDataReader.GetChar(ordinal);
        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) => _innerDbDataReader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
        public override string GetDataTypeName(int ordinal) => _innerDbDataReader.GetDataTypeName(ordinal);
        public override DateTime GetDateTime(int ordinal) => _innerDbDataReader.GetDateTime(ordinal);
        public override decimal GetDecimal(int ordinal) => _innerDbDataReader.GetDecimal(ordinal);
        public override double GetDouble(int ordinal) => _innerDbDataReader.GetDouble(ordinal);

        public override IEnumerator GetEnumerator() => ((IEnumerable)_innerDbDataReader).GetEnumerator();
        public override Type GetFieldType(int ordinal) => _innerDbDataReader.GetFieldType(ordinal);
        public override T GetFieldValue<T>(int ordinal) => _innerDbDataReader.GetFieldValue<T>(ordinal);
        public override Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken) => _innerDbDataReader.GetFieldValueAsync<T>(ordinal, cancellationToken);
        public override float GetFloat(int ordinal) => _innerDbDataReader.GetFloat(ordinal);
        public override Guid GetGuid(int ordinal) => _innerDbDataReader.GetGuid(ordinal);

        public override short GetInt16(int ordinal) => _innerDbDataReader.GetInt16(ordinal);
        public override int GetInt32(int ordinal) => _innerDbDataReader.GetInt32(ordinal);
        public override long GetInt64(int ordinal) => _innerDbDataReader.GetInt64(ordinal);
        public override string GetName(int ordinal) => _innerDbDataReader.GetName(ordinal);
        public override int GetOrdinal(string name) => _innerDbDataReader.GetOrdinal(name);
        public override Type GetProviderSpecificFieldType(int ordinal) => _innerDbDataReader.GetProviderSpecificFieldType(ordinal);
        public override object GetProviderSpecificValue(int ordinal) => _innerDbDataReader.GetProviderSpecificValue(ordinal);
        public override int GetProviderSpecificValues(object[] values) => _innerDbDataReader.GetProviderSpecificValues(values);
        public override DataTable GetSchemaTable() => _innerDbDataReader.GetSchemaTable();
        public override Task<DataTable> GetSchemaTableAsync(CancellationToken cancellationToken = default) => _innerDbDataReader.GetSchemaTableAsync(cancellationToken);
        public override Stream GetStream(int ordinal) => _innerDbDataReader.GetStream(ordinal);
        public override string GetString(int ordinal) => _innerDbDataReader.GetString(ordinal);
        public override TextReader GetTextReader(int ordinal) => _innerDbDataReader.GetTextReader(ordinal);
        public override object GetValue(int ordinal) => _innerDbDataReader.GetValue(ordinal);
        public override int GetValues(object[] values) => _innerDbDataReader.GetValues(values);
        public override bool IsDBNull(int ordinal) => _innerDbDataReader.IsDBNull(ordinal);
        public override Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken) => _innerDbDataReader.IsDBNullAsync(ordinal, cancellationToken);
        public override bool NextResult() => _innerDbDataReader.NextResult();
        public override Task<bool> NextResultAsync(CancellationToken cancellationToken) => _innerDbDataReader.NextResultAsync(cancellationToken);
        public override bool Read()
        {
            var hasMoreRows = _innerDbDataReader.Read();
            if (hasMoreRows)
            {
                _rowsRead++;
            }
            return hasMoreRows;
        }

        public override async Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            var hasMoreRows = await _innerDbDataReader.ReadAsync(cancellationToken);
            if (hasMoreRows)
            {
                _rowsRead++;
            }
            return await _innerDbDataReader.ReadAsync(cancellationToken);
        }

        public override void Close()
        {
            _innerDbDataReader.Close();
            _readerActivity.AddTag(CustomTagNames.RowsRead, _rowsRead.ToString());
            _readerActivity?.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            _innerDbDataReader.Dispose();
            base.Dispose(disposing);
        }
    }
}