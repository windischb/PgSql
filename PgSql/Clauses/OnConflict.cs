using System;
using System.Collections.Generic;
using System.Linq;
using doob.PgSql.Interfaces;

namespace doob.PgSql.Clauses
{
    public class OnConflict : IInsertMember, IDeleteMember
    {

        private string ConflictAction { get; set; } = "THROW";
        private List<IValues> _valueClauses = new List<IValues>();

        public static OnConflict DoUpdate() {
            var oc = new OnConflict();
            oc.ConflictAction = "DO UPDATE";
            return oc;
        }

        public static OnConflict DoNothing()
        {
            var oc = new OnConflict();
            oc.ConflictAction = "DO NOTHING";
            return oc;
        }

        private OnConflict()
        {
        }


        

        internal OnConflict Values(List<IValues> values) {
            _valueClauses = values;
            return this;
        }


        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();

            if (ConflictAction == "THROW")
                return sqlCommand;

            sqlCommand.AppendCommand($"ON CONFLICT");

            if (ConflictAction == "DO NOTHING") {
                sqlCommand.AppendCommandLine(" DO NOTHING");
                return sqlCommand;
            }

            sqlCommand.AppendCommandLine($" ({String.Join(", ", tableDefinition.PrimaryKeys().Select(c => c.Properties.Name))})");

            sqlCommand.AppendCommandLine(" DO UPDATE SET");

            if (_valueClauses.Any())
            {
                foreach (var valuesClause in _valueClauses)
                {
                    if (valuesClause is NamedValues namedValues)
                    {
                        
                        foreach (var namedValuesValue in namedValues._values) {
                            var expr = new PgSqlParameter(namedValuesValue._key, namedValuesValue._value).SetColum(tableDefinition);
                            sqlCommand.AppendCommandLine($"{namedValuesValue._key} = {ValuesClauseHelper.GetValuePlaceholder(expr)}", new List<PgSqlParameter>(){ expr});
                        }
                    }
                    else if (valuesClause is PositionedValues positionedValues)
                    {
                         foreach (var positionedValue in positionedValues._values) {
                            var expr = new PgSqlParameter(positionedValue._key, positionedValue._value);
                            sqlCommand.AppendCommandLine($"{positionedValue._key} = {ValuesClauseHelper.GetValuePlaceholder(expr)}", new List<PgSqlParameter>(){ expr});
                        }
                    }
                }
                sqlCommand.Command = sqlCommand.Command.TrimEnd($",{Environment.NewLine}".ToCharArray()) + Environment.NewLine;
            }

            return sqlCommand;
        }
    }
}
