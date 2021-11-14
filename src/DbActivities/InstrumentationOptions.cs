using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;

namespace DbActivities
{
    /// <summary>
    /// Represents the various options to be used when instrumenting.
    /// </summary>
    public class InstrumentationOptions
    {
        private readonly List<Action<DbCommand>> _commandActions = new();

        private Func<DbCommand, string> _formatCommantText;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstrumentationOptions"/> class.
        /// </summary>
        /// <param name="system">The name of the system to be used.</param>
        public InstrumentationOptions(string system = "other_sql")
        {
            ActivitySource = Assembly.GetCallingAssembly().CreateActivitySource();
            _activityStarter = (source, name) => source.StartActivity(name, ActivityKind.Client);
            _formatCommantText = (c) => c.CommandText;
            System = system;
        }

        /// <summary>
        /// Gets or sets the <see cref="ActivitySource"/> to be used for creating new activities.
        /// </summary>
        public ActivitySource ActivitySource { get; set; }

        private Func<ActivitySource, string, Activity?> _activityStarter;

        /// <summary>
        /// Gets the system according to being reported as "db.system".
        /// </summary>
        public string System { get; }

        /// <summary>
        /// Gets or sets the user to be reported as "db.user".
        /// </summary>
        public string User { get; set; }

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

        /// <summary>
        /// Use this method to override the default behavior when formatting <see cref="DbCommand.CommandText"/>.
        /// The default is simply to return the value from <see cref="DbCommand.CommandText"/>.
        /// </summary>
        /// <typeparam name="TCommand">The command for which to format the command text.</typeparam>
        /// <param name="format">A function for formatting the command text.</param>
        /// <returns>This <see cref="InstrumentationOptions"/> for chaining method calls.</returns>
        public InstrumentationOptions FormatCommandText<TCommand>(Func<TCommand, string> format) where TCommand : DbCommand
        {
            _formatCommantText = c => format((TCommand)c);
            return this;
        }

        /// <summary>
        /// Allows configuration of the <see cref="DbCommand"/> just before it is executed.
        /// </summary>
        /// <typeparam name="TCommand">The type of <see cref="DbCommand"/> to configure.</typeparam>
        /// <param name="configureCommand">A function to configure the <see cref="DbCommand"/>.</param>
        /// <returns>This <see cref="InstrumentationOptions"/> for chaining method calls.</returns>
        public InstrumentationOptions ConfigureDbCommand<TCommand>(Action<TCommand> configureCommand) where TCommand : DbCommand
        {
            Action<DbCommand> commandAction = c => configureCommand((TCommand)c);
            _commandActions.Add(commandAction);
            return this;
        }

        /// <summary>
        /// Allows custom configuration of starting activities.
        /// </summary>
        /// <param name="starter">A function used to start an <see cref="Activity"/>.</param>
        /// <returns>This <see cref="InstrumentationOptions"/> for chaining method calls.</returns>
        public InstrumentationOptions ConfigureActivityStarter(Func<ActivitySource, string, Activity?> starter)
        {
            _activityStarter = starter;
            return this;
        }
    }
}