namespace doob.PgSql.CustomTypes
{
    public sealed class PgSqlLTree : IPgSqlType
    {
        private string _value = string.Empty;

        public string PostgresType { get; } = "ltree";

        public PgSqlLTree()
        {
        }

        public PgSqlLTree(string value)
        {
            this._value = value;
        }

        public static implicit operator string(PgSqlLTree value)
        {
            return value._value;
        }

        public static implicit operator PgSqlLTree(string value)
        {
            return new PgSqlLTree(value);
        }

        public override string ToString()
        {
            return this._value;
        }
    }
}
