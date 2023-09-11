using System;
using System.Collections;
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

        private readonly Activity _readerActivity;

        private int _rowsRead = 0;

        public InstrumentedDbDataReader(DbDataReader dbDataReader, InstrumentationOptions options)
        {
            _innerDbDataReader = dbDataReader;
            _options = options;
            _readerActivity = options.StartActivity($"{nameof(InstrumentedDbDataReader)}");
        }

        /// <summary>
        /// Get the "inner" <see cref="DbDataReader"/> that is being instrumented.
        /// </summary>
        public DbDataReader InnerDbDataReader { get => _innerDbDataReader; }

        /// <inheritdoc/>
        public override object this[int ordinal] => _innerDbDataReader[ordinal];

        /// <inheritdoc/>
        public override object this[string name] => _innerDbDataReader[name];

        /// <inheritdoc/>
        public override int Depth => _innerDbDataReader.Depth;

        /// <inheritdoc/>
        public override int FieldCount => _innerDbDataReader.FieldCount;

        /// <inheritdoc/>
        public override bool HasRows => _innerDbDataReader.HasRows;

        /// <inheritdoc/>
        public override bool IsClosed => _innerDbDataReader.IsClosed;

        /// <inheritdoc/>
        public override int RecordsAffected => _innerDbDataReader.RecordsAffected;

        /// <inheritdoc/>
        public override bool GetBoolean(int ordinal) => _innerDbDataReader.GetBoolean(ordinal);

        /// <inheritdoc/>
        public override byte GetByte(int ordinal) => _innerDbDataReader.GetByte(ordinal);

        /// <inheritdoc/>
        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) => _innerDbDataReader.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);

        /// <inheritdoc/>
        public override char GetChar(int ordinal) => _innerDbDataReader.GetChar(ordinal);

        /// <inheritdoc/>
        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) => _innerDbDataReader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);

        /// <inheritdoc/>
        public override string GetDataTypeName(int ordinal) => _innerDbDataReader.GetDataTypeName(ordinal);

        /// <inheritdoc/>
        public override DateTime GetDateTime(int ordinal) => _innerDbDataReader.GetDateTime(ordinal);

        /// <inheritdoc/>
        public override decimal GetDecimal(int ordinal) => _innerDbDataReader.GetDecimal(ordinal);

        /// <inheritdoc/>
        public override double GetDouble(int ordinal) => _innerDbDataReader.GetDouble(ordinal);

        /// <inheritdoc/>
        public override IEnumerator GetEnumerator() => ((IEnumerable)_innerDbDataReader).GetEnumerator();

        /// <inheritdoc/>
        public override Type GetFieldType(int ordinal) => _innerDbDataReader.GetFieldType(ordinal);

        /// <inheritdoc/>
        public override T GetFieldValue<T>(int ordinal) => _innerDbDataReader.GetFieldValue<T>(ordinal);

        /// <inheritdoc/>
        public override Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken) => _innerDbDataReader.GetFieldValueAsync<T>(ordinal, cancellationToken);

        /// <inheritdoc/>
        public override float GetFloat(int ordinal) => _innerDbDataReader.GetFloat(ordinal);

        /// <inheritdoc/>
        public override Guid GetGuid(int ordinal) => _innerDbDataReader.GetGuid(ordinal);

        /// <inheritdoc/>
        public override short GetInt16(int ordinal) => _innerDbDataReader.GetInt16(ordinal);

        /// <inheritdoc/>
        public override int GetInt32(int ordinal) => _innerDbDataReader.GetInt32(ordinal);

        /// <inheritdoc/>
        public override long GetInt64(int ordinal) => _innerDbDataReader.GetInt64(ordinal);

        /// <inheritdoc/>
        public override string GetName(int ordinal) => _innerDbDataReader.GetName(ordinal);

        /// <inheritdoc/>
        public override int GetOrdinal(string name) => _innerDbDataReader.GetOrdinal(name);

        /// <inheritdoc/>
        public override DataTable GetSchemaTable() => _innerDbDataReader.GetSchemaTable();

        /// <inheritdoc/>
        public override Task<DataTable> GetSchemaTableAsync(CancellationToken cancellationToken = default) => _innerDbDataReader.GetSchemaTableAsync(cancellationToken);

        /// <inheritdoc/>
        public override Stream GetStream(int ordinal) => _innerDbDataReader.GetStream(ordinal);

        /// <inheritdoc/>
        public override string GetString(int ordinal) => _innerDbDataReader.GetString(ordinal);

        /// <inheritdoc/>
        public override TextReader GetTextReader(int ordinal) => _innerDbDataReader.GetTextReader(ordinal);

        /// <inheritdoc/>
        public override object GetValue(int ordinal) => _innerDbDataReader.GetValue(ordinal);

        /// <inheritdoc/>
        public override int GetValues(object[] values) => _innerDbDataReader.GetValues(values);

        /// <inheritdoc/>
        public override bool IsDBNull(int ordinal) => _innerDbDataReader.IsDBNull(ordinal);

        /// <inheritdoc/>
        public override Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken) => _innerDbDataReader.IsDBNullAsync(ordinal, cancellationToken);

        /// <inheritdoc/>
        public override bool NextResult() => _innerDbDataReader.NextResult();

        /// <inheritdoc/>
        public override Task<bool> NextResultAsync(CancellationToken cancellationToken) => _innerDbDataReader.NextResultAsync(cancellationToken);

        /// <inheritdoc/>
        public override bool Read()
        {
            var hasMoreRows = _innerDbDataReader.Read();
            if (hasMoreRows)
            {
                _rowsRead++;
            }
            return hasMoreRows;
        }

        /// <inheritdoc/>
        public override async Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            var hasMoreRows = await _innerDbDataReader.ReadAsync(cancellationToken);
            if (hasMoreRows)
            {
                _rowsRead++;
            }
            return hasMoreRows;
        }

        /// <inheritdoc/>
        public override void Close()
        {
            _readerActivity?.AddTag(CustomTagNames.RowsRead, _rowsRead.ToString());
            _options.ConfigureActivityInternal(_readerActivity);
            _options.ConfigureDataReaderActivityInternal(_readerActivity, _innerDbDataReader);
            _innerDbDataReader.Close();
            _readerActivity?.Dispose();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            _innerDbDataReader.Dispose();
            base.Dispose(disposing);
        }
    }
}