namespace doob.PgSql.Attributes
{
    public class PgSqlAliasAttribute : PgSqlValueAttribute
    {

        public PgSqlAliasAttribute(string value)
        {
            Value = value;
        }
    }
}
