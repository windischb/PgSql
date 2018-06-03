using System;
using NpgsqlTypes;

namespace PgSql2
{
    internal class TypeMappingAttribute
    {
        public string PgName { get; set; }
        public NpgsqlDbType? NpgsqlDbType { get; set; }
        public Type[] ClrTypes { get; set; }
        public string PgTypeNeeded { get; set; }
    }
}
