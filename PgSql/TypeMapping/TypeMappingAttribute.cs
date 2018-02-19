using System;
using NpgsqlTypes;

namespace doob.PgSql.TypeMapping
{
    internal class TypeMappingAttribute
    {
        public string PgName { get; set; }
        public NpgsqlDbType? NpgsqlDbType { get; set; }
        public Type[] ClrTypes { get; set; }
        public string PgTypeNeeded { get; set; }
    }
}
