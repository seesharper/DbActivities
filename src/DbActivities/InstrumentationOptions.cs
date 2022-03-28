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
        private readonly List<Action<Activity>> _activityActions = new();

        private readonly List<Action<Activity, DbCommand>> _configureCommandActions = new();

        private readonly List<Action<Activity, DbDataReader>> _configureDataReaderActions = new();

        private readonly List<Action<Activity, DbConnection>> _configureConnectionActions = new();

        private readonly List<Action<Activity, DbTransaction>> _configureTransactionActions = new();

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

        internal void ConfigureCommandActivityInternal(Activity activity, DbCommand dbCommand)
        {
            foreach (var action in _configureCommandActions)
            {
                action(activity, dbCommand);
            }
        }

        internal void ConfigureDataReaderActivityInternal(Activity activity, DbDataReader dbDataReader)
        {
            foreach (var action in _configureDataReaderActions)
            {
                action(activity, dbDataReader);
            }
        }

        internal void ConfigureTransactionActivityInternal(Activity activity, DbTransaction dbTransaction)
        {
            foreach (var action in _configureTransactionActions)
            {
                action(activity, dbTransaction);
            }
        }

        internal void ConfigureConnectionActivityInternal(Activity activity, DbConnection dbConnection)
        {
            foreach (var action in _configureConnectionActions)
            {
                action(activity, dbConnection);
            }
        }

        internal void ConfigureActivityInternal(Activity activity)
        {
            foreach (var action in _activityActions)
            {
                action(activity);
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

        public InstrumentationOptions ConfigureCommandActivity<TCommand>(Action<Activity, TCommand> configureCommandActivity) where TCommand : DbCommand
        {
            Action<Activity, DbCommand> configureAction = (activity, command) => configureCommandActivity(activity, (TCommand)command);
            _configureCommandActions.Add(configureAction);
            return this;
        }

        public InstrumentationOptions ConfigureDataReaderActivity<TDataReader>(Action<Activity, TDataReader> configureDataReaderActivity) where TDataReader : DbDataReader
        {
            Action<Activity, DbDataReader> configureAction = (activity, dataReader) => configureDataReaderActivity(activity, (TDataReader)dataReader);
            _configureDataReaderActions.Add(configureAction);
            return this;
        }

        public InstrumentationOptions ConfigureConnectionActivity<TConnection>(Action<Activity, TConnection> configureDataReaderActivity) where TConnection : DbConnection
        {
            Action<Activity, DbConnection> configureAction = (activity, connection) => configureDataReaderActivity(activity, (TConnection)connection);
            _configureConnectionActions.Add(configureAction);
            return this;
        }

        public InstrumentationOptions ConfigureTransactionActivity<TTransaction>(Action<Activity, TTransaction> configureDataReaderActivity) where TTransaction : DbTransaction
        {
            Action<Activity, DbTransaction> configureAction = (activity, transaction) => configureDataReaderActivity(activity, (TTransaction)transaction);
            _configureTransactionActions.Add(configureAction);
            return this;
        }

        public InstrumentationOptions ConfigureActivity(Action<Activity> configureActivity)
        {
            _activityActions.Add(configureActivity);
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