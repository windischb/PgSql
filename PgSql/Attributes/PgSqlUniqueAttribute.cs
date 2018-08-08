namespace doob.PgSql.Attributes
{
    public class PgSqlUniqueAttribute : PgSqlValueAttribute
    {
        public string Group { get; set; }

        public PgSqlUniqueAttribute()
        {
            
        }

        public PgSqlUniqueAttribute(string group)
        {
            Group = group;
        }

    }
}
