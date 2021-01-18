namespace KsqlDb.Api.Client.Abstractions.Objects
{
    public class KSqlNull
    {
        public static readonly KSqlNull Instance = new KSqlNull();

        private KSqlNull()
        {
        }
    }
}
