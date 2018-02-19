using System;

namespace doob.PgSql.CustomTypes
{
    public sealed class PgSqlSecureString : IPgSqlType
    {

        public string PostgresType { get; } = "secstring";

        private string _value = String.Empty;

        public PgSqlSecureString() { }

        public PgSqlSecureString(string value)
        {
            _value = value;
        }

        public static implicit operator String(PgSqlSecureString value)
        {
            return value._value;
        }

        public static implicit operator PgSqlSecureString(String value)
        {
            return new PgSqlSecureString(value);
        }


        public override string ToString()
        {
            return _value;
        }
    }
}
