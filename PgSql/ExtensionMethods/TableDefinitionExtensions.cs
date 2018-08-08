using System;
using System.Collections.Generic;
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

            var uniqueGroups = new Dictionary<string, List<string>>();

            var strBuilder = new StringBuilder();
            tblDefinition.Columns().ToList().ForEach(c =>
            {
                string typ = c.PgType ?? PgSqlTypeManager.Global.GetPostgresName(c.DotNetType);
                if (c.DotNetType.IsEnum)
                {
                    typ = "Text";
                }
                
                var str = $"\"{c.GetNameForDb()}\" {typ}".Trim();
                if (!String.IsNullOrWhiteSpace(c.DefaultValue))
                {
                    if (c.DefaultValue.ToLower() == "bigserial")
                    {
                        str = $"{str.TrimEnd(typ.ToCharArray())} BIGSERIAL";
                    }
                    else
                    {
                        str = $"{str} DEFAULT {c.DefaultValue}".Trim();
                    }

                }


                if (c.MustBeUnique)
                {
                    if(!String.IsNullOrWhiteSpace(c.UniqueGroup))
                    {
                        if(uniqueGroups.ContainsKey(c.UniqueGroup))
                        {
                            uniqueGroups[c.UniqueGroup].Add($"\"{c.GetNameForDb()}\"");
                        }
                        else
                        {
                            uniqueGroups.Add(c.UniqueGroup, new List<string> { $"\"{c.GetNameForDb()}\"" });
                        }
                    } else
                    {
                        str = $"{str} UNIQUE".Trim();
                    }
                    
                }
                    

                if (!c.CanBeNull || c.IsPrimaryKey)
                    str = $"{str} NOT NULL".Trim();

                strBuilder.AppendLine($"    {str},");
            });

            if (tblDefinition.PrimaryKeys() != null)
            {
                var keys = String.Join(", ", tblDefinition.PrimaryKeys().Select(p => $"\"{p.GetNameForDb()}\""));
                strBuilder.Append($"    PRIMARY KEY ({keys})");
            }

            uniqueGroups.Keys.ToList().ForEach(k =>
            {
                strBuilder.AppendLine(",");
                strBuilder.AppendLine($"    UNIQUE ({String.Join(", ", uniqueGroups[k])})");

            });

            return strBuilder.ToString();
        }
    }
}
