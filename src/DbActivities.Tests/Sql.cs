namespace DbActivities.Tests
{
    public static class Sql
    {
        public static string CreateTestTable { get => "CREATE TABLE TestTable (Id int null)"; }

        public static string Rubbish { get => "Rubbish"; }

        public static string GetCount { get => "SELECT COUNT(*) FROM TestTable"; }
    }
}
