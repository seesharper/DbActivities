## DbActivities

**DbActivities** is a library that instruments implementations of the following classes from the `System.Data.Common` namespace.

* DbConnection
* DbTransaction
* DbCommand
* DbDataReader

### How it works

**DbActivities** is implemented using the Decorator pattern mening that it sits between our code and the actual implementations.
This makes it possible to intercept a call to `ExecuteNonQuery` and report information about that call.  This information is typically the duration of the call, the SQL statement being executed and so on. Information is collected and reported using the `Activity` class from the `System.Diagnostics` namespace. The advantage of this is that `DbActivities` has no third-party dependencies and we still support things like distributed tracing and exporting OpenTelemetry data.

### OpenTelemetry

OpenTelemetry is an effort to standardise how we export traces, logs and metrics from our applications. This is a huge improvement since we can get rid of any vendor-specific code and just publish our data to an endpoint that understands the OpenTelemetry protocol (OTLP).  

![](decorator.drawio.svg)



### Example

```c#
var sqliteConnection = new SqliteConnection();
// Set things like the ConnectionString and stuff here
sql
var instrumentationOptions = new InstrumentationOptions(source:"sqlite");
var dbConnection = new InstrumentedDbConnection()
```











