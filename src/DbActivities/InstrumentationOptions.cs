using System;
using System.Data.Common;
using System.Diagnostics;

namespace DbActivities
{
    public class InstrumentationOptions
    {
        public InstrumentationOptions(string system)
        {
            ActivitySource = typeof(InstrumentationOptions).Assembly.CreateActivitySource();
            System = system;
        }

        public ActivitySource ActivitySource { get; set; }

        public string System { get; }
        public string User { get; }

        public Action<DbCommand> ConfigureDbCommand { get; set; }
    }

}