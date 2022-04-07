using System.Data.Common;

namespace DbActivities.Tests
{
    public class CustomInstrumentedDbCommand : InstrumentedDbCommand
    {
        public CustomInstrumentedDbCommand(DbCommand dbCommand, DbConnection dbConnection, InstrumentationOptions options) : base(dbCommand, dbConnection, options)
        {
        }
    }
}