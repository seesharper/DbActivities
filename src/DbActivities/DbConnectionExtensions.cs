using System;
using System.Data.Common;

namespace DbActivities
{
    public static class DbConnectionExtensions
    {
        public static InstrumentedDbConnection AsInstrumentedDbConnection(this DbConnection dbConnection, Action<InstrumentationOptions> config)
        {
            var options = new InstrumentationOptions();
            config(options);
            return new InstrumentedDbConnection(dbConnection, options);
        }
    }
}