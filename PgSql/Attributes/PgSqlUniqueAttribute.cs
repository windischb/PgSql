namespace doob.PgSql.Attributes
{
    public class PgSqlUniqueAttribute : PgSqlValueAttribute
    {
        public PgSqlUniqueAttribute()
        {
            Value = true;
        }

        public PgSqlUniqueAttribute(bool unique) {
            Value = unique;
        }
    }
}
