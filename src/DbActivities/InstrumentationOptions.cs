using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;

namespace DbActivities
{
    public class InstrumentationOptions
    {
        private readonly List<Action<DbCommand>> _commandActions = new();

        public InstrumentationOptions(string system = "other_sql")
        {
            ActivitySource = Assembly.GetCallingAssembly().CreateActivitySource();
            _activityStarter = (source, name) => source.StartActivity(name, ActivityKind.Client);
            System = system;
        }

        public ActivitySource ActivitySource { get; set; }

        private Func<ActivitySource, string, Activity?> _activityStarter;

        public string System { get; }
        public string User { get; }

        internal void ConfigureDbCommandInternal(DbCommand dbCommand)
        {
            foreach (var action in _commandActions)
            {
                action(dbCommand);
            }
        }

        internal Activity StartActivity(string name)
        {
            return _activityStarter(ActivitySource, name);
        }

        public InstrumentationOptions ConfigureDbCommand<TCommand>(Action<TCommand> configureCommand) where TCommand : DbCommand
        {
            Action<DbCommand> commandAction = c =>
            {
                configureCommand((TCommand)c);
            };
            _commandActions.Add(commandAction);

            return this;
        }

        public InstrumentationOptions ConfigureActivityStarter(Func<ActivitySource, string, Activity?> starter)
        {
            _activityStarter = starter;
            return this;
        }
    }

}