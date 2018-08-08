namespace doob.PgSql.Attributes
{
    public class PgSqlColumnAttribute : PgSqlAttribute {

        public string Name { get; }
        public string UseExtension { get; }

        public PgSqlColumnAttribute(string name = null, string useExtension = null)
        {
            Name = name;
            UseExtension = useExtension;
        }
    }
}
