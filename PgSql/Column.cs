using System;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.TypeMapping;
using Newtonsoft.Json;
using NpgsqlTypes;


namespace doob.PgSql
{
    public class Column
    {


        [JsonProperty]
        public int? Position { get; internal set; }

        [JsonProperty]
        public string ClrName { get; internal set; }

        [JsonProperty("column_name")]
        public string DbName { get; set; }

        [JsonProperty("is_nullable")]
        public bool CanBeNull { get; internal set; } = true;

        [JsonProperty("is_unique")]
        public bool MustBeUnique { get; internal set; }

        public string UniqueGroup { get; internal set; }

        [JsonProperty("is_primarykey")]
        public bool IsPrimaryKey { get; internal set; }

        [JsonProperty("column_default")]
        public string DefaultValue { get; internal set; }


        [JsonProperty]
        public Type DotNetType { get; internal set; }

        [JsonProperty("pg_type")]
        public string PgType { get; set; }

        [JsonProperty]
        public string NeedsExtension { get; set; }

        [JsonIgnore]
        private NpgsqlDbType? _npgsqlDbType;

        internal NpgsqlDbType? GetNpgSqlDbType() => _npgsqlDbType ?? (_npgsqlDbType = !String.IsNullOrWhiteSpace(PgType) ? PgSqlTypeManager.Global.GetNpgsqlDbType(PgType) : PgSqlTypeManager.Global.GetNpgsqlDbType(DotNetType));


        internal string GetNameForDb()
        {
            return DbName.ToNull() ?? ClrName.ToNull();
        }

        public ColumnBuilder Builder()
        {
            return new ColumnBuilder(this);
        }


        public static ColumnBuilder Build(string name, Type dotnetType)
        {
            return ColumnBuilder.Build(name, dotnetType);
        }
        public static ColumnBuilder Build(string name, string typeName)
        {
            return ColumnBuilder.Build(name, typeName);
        }
        public static ColumnBuilder Build<T>(string name)
        {
            return ColumnBuilder.Build<T>(name);
        }



    }
}
