namespace doob.PgSql.Attributes
{
    public class PgSqlSetOnUpdateAttribute : PgSqlAttribute
    {
        public string OnUpdate { get; private set; }

        public PgSqlSetOnUpdateAttribute(string onUpdate)
        {
            OnUpdate = onUpdate;
        }
    }
}
