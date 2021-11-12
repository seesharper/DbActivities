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

        private Func<DbCommand, string> _formatCommantText;

        public InstrumentationOptions(string system = "other_sql")
        {
            ActivitySource = Assembly.GetCallingAssembly().CreateActivitySource();
            _activityStarter = (source, name) => source.StartActivity(name, ActivityKind.Client);
            _formatCommantText = (c) => c.CommandText;
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

        internal string FormatCommandTextInternal(DbCommand dbCommand)
        {
            return _formatCommantText(dbCommand);
        }

        internal Activity StartActivity(string name)
        {
            return _activityStarter(ActivitySource, name);
        }

        public InstrumentationOptions FormatCommandText<TCommand>(Func<TCommand, string> format) where TCommand : DbCommand
        {
            _formatCommantText = c =>
            {
                return format((TCommand)c);
            };

            return this;
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