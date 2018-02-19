namespace doob.PgSql.Attributes
{
    public class PgSqlColumnAttribute : PgSqlAttribute {

        public string Name { get; }

        public PgSqlColumnAttribute(string name)
        {
            Name = name;
        }
    }
}
