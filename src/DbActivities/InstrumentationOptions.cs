using System;
using System.Data.Common;
using System.Diagnostics;

namespace DbActivities
{
    public class InstrumentationOptions
    {
        public InstrumentationOptions(string system)
        {
            ActivitySource = new ActivitySource(typeof(InstrumentationOptions).Assembly.GetName().Name, "0.0.1");
            System = system;
        }

        public ActivitySource ActivitySource { get; set; }

        public string System { get; }
        public string User { get; }

        public Action<DbCommand> ConfigureDbCommand { get; set; }
    }

}