using System;
using System.Linq;
using System.Text;
using doob.PgSql.TypeMapping;

namespace doob.PgSql.ExtensionMethods
{
    public static class TableDefinitionExtensions
    {
        
        public static string GetSqlDefinition(this TableDefinition tblDefinition, string tableName, string schemaName, bool throwIfAlreadyExists)
        {
            schemaName = $"\"{schemaName.Trim("\"".ToCharArray())}\"";
            tableName = $"\"{tableName.Trim("\"".ToCharArray())}\"";

            var strBuilder = new StringBuilder();
            strBuilder.Append("CREATE TABLE");
            if (!throwIfAlreadyExists)
            {
                strBuilder.Append(" IF NOT EXISTS");
            }
            strBuilder.AppendLine($" {(!String.IsNullOrWhiteSpace(schemaName) ? $"{schemaName}." : "")}{tableName} (");
            strBuilder.AppendLine(GetInnerSqlDefinition(tblDefinition));
            strBuilder.Append(");");
            return strBuilder.ToString();
        }

        public static string GetInnerSqlDefinition(this TableDefinition tblDefinition)
        {

            var strBuilder = new StringBuilder();
            tblDefinition.Columns().ToList().ForEach(c =>
            {
                var typ = c.Properties.CustomDbType ?? PgSqlTypeManager.GetPostgresName(c.Properties.DotNetType);

                var str = $"\"{c.Properties.Name}\" {typ}".Trim();
                if (!String.IsNullOrWhiteSpace(c.Properties.DefaultValue))
                {
                    if (c.Properties.DefaultValue.ToLower() == "bigserial")
                    {
                        str = $"{str.TrimEnd(typ.ToCharArray())} BIGSERIAL";
                    }
                    else
                    {
                        str = $"{str} DEFAULT {c.Properties.DefaultValue}".Trim();
                    }

                }


                if (c.Properties.Unique)
                    str = $"{str} UNIQUE".Trim();

                if (!c.Properties.Nullable || c.Properties.PrimaryKey)
                    str = $"{str} NOT NULL".Trim();

                strBuilder.AppendLine($"    {str},");
            });

            if (tblDefinition.PrimaryKeys() != null)
            {
                var keys = String.Join(", ", tblDefinition.PrimaryKeys().Select(p => $"\"{p.Properties.Name}\""));
                strBuilder.AppendLine($"    PRIMARY KEY ({keys})");
            }

            return strBuilder.ToString();
        }
    }
}
