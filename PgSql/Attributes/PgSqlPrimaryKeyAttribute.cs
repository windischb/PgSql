namespace doob.PgSql.Attributes
{
    public class PgSqlPrimaryKeyAttribute : PgSqlValueAttribute
    {

        public PgSqlPrimaryKeyAttribute()
        {
            Value = null;
        }

        public PgSqlPrimaryKeyAttribute(string defaultValue) : this()
        {
            Value = defaultValue;
        }
    }
}
