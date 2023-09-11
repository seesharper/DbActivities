using System;
using System.Data.Common;

namespace DbActivities
{
    public static class DbConnectionExtensions
    {
        /// <summary>
        /// Wraps the <see cref="DbConnection"/> in an <see cref="InstrumentedDbConnection"/> instance.
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/> to be wrapped as an <see cref="InstrumentedDbConnection"/>.</param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static InstrumentedDbConnection AsInstrumentedDbConnection(this DbConnection dbConnection, Action<InstrumentationOptions> config)
        {
            var options = new InstrumentationOptions();
            config(options);
            return new InstrumentedDbConnection(dbConnection, options);
        }
    }
}