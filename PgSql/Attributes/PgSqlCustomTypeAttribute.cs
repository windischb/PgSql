namespace doob.PgSql.Attributes
{

    public class PgSqlCustomTypeAttribute : PgSqlValueAttribute
    {
        public PgSqlCustomTypeAttribute(string value)
        {
            Value = value;
        }
    }
}
