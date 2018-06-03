using System;
using System.Collections.Generic;
using System.Linq;
using doob.PgSql.Clauses;
using doob.PgSql.Interfaces;

namespace doob.PgSql.Statements
{
    public class Insert : ISQLCommand
    {
        private IIntoDestination _intoDestination;
        private IntoColumns _intoColumns;
        private List<IValues> _valueClauses = new List<IValues>();
        private Returning _returningClause;
        private OnConflict _onConflict;

        public Insert(string table)
        {
            _intoDestination = new IntoDestinationTable(table);
        }
        public Insert(string table, string alias)
        {
            _intoDestination = new IntoDestinationTable(table, alias);
        }

        public static Insert Into(string name)
        {
            return new Insert(name);
        }
        public static Insert Into(string name, string alias)
        {
            return new Insert(name, alias);
        }

        public Insert SetColumns(params string[] columns)
        {
            return AddClause(IntoColumns.Names(columns));
        }
        public Insert SetColumns(IntoColumns intoColumnsClause)
        {
            return AddClause(intoColumnsClause);
        }

        public Insert AddColumnsFromTableDefinition(TableDefinition tableDefinition)
        {
            if(_intoColumns == null)
                _intoColumns = IntoColumns.Create();

            foreach (var column in tableDefinition.Columns())
            {
               _intoColumns.AddColumn(column.Name);
            }
            return this;
        }

        public Insert AddPositionedValues(params object[] values)
        {
            var vc = PositionedValues.Create();
            foreach (var o in values)
            {
                vc.AddValue(o);
            }
            return AddClause(vc);
        }

        public Insert AddNamedValues(params KeyValuePair<string, object>[] values)
        {
            var vc = NamedValues.Create();
            foreach (var o in values)
            {
                var val = o.Value;
                vc.AddValue(o.Key, val);
            }
            return AddClause(vc);
        }

        public Insert AddValuesFromObject(object @object)
        {
            //Dictionary<string, object> tempDict = null;
            //if (@object is JObject)
            //    tempDict = ((JObject)@object).ToObject<Dictionary<string, object>>();

            //if (tempDict == null)
            //{
            //    var dict = @object as IDictionary<string, object>;
            //    if (dict != null)
            //    {
            //        tempDict = new Dictionary<string, object>(dict, StringComparer.OrdinalIgnoreCase);
            //    }
            //    else
            //    {
            //        tempDict = @object.GetType().GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance).ToDictionary(
            //            (p) => p.Name,
            //            (p) => p.GetValue(@object),
            //            StringComparer.OrdinalIgnoreCase);
            //    }
            //}

            return AddNamedValues(global::doob.PgSql.JSON.ToDictionary(@object).ToArray());
        }

        public Insert Returning(Returning returning)
        {
            return AddClause(returning);
        }

        public Insert AddClause(IInsertMember clause)
        {
            switch (clause)
            {
                case IntoColumns ic:
                    _intoColumns = ic;
                    break;
                case IValues val:
                    _valueClauses.Add(val);
                    break;
                case Returning ret:
                    _returningClause = ret;
                    break;
            }

            return this;
        }

        public Insert OnConflict(OnConflict onConflict) {
            _onConflict = onConflict;
            _onConflict.Values(_valueClauses);
            return this;
        }

        public PgSqlCommand GetSqlCommand()
        {
            return GetSqlCommand(null);
        }

        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();

            TableDefinition keyDefinition = new TableDefinition();

            sqlCommand.AppendCommand($"INSERT INTO {String.Join(", ", _intoDestination.GetSqlCommand(tableDefinition).Command)}");

            if (_intoColumns != null)
            {
                var intoColumnsComm = _intoColumns.GetSqlCommand(tableDefinition);
                sqlCommand.AppendCommandLine($"{Environment.NewLine}\t({intoColumnsComm.Command})");

                _intoColumns._columns.ForEach(c =>
                {
                    var col = tableDefinition?.GetColumn(c._name);
                    if (col == null)
                    {
                        keyDefinition.AddColumn(new ColumnBuilder().SetName(c._name));
                    }
                    else
                    {
                        keyDefinition.AddColumn(col);
                    }
                });
            }

            if (_valueClauses.Any())
            {

                sqlCommand.AppendCommandLine($"VALUES");

                foreach (var valuesClause in _valueClauses)
                {
                    if (valuesClause is NamedValues)
                    {
                        var values = valuesClause as NamedValues;
                        var valuesComm = valuesClause.GetSqlCommand(keyDefinition);
                        sqlCommand.AppendCommandLine($"\t({valuesComm.Command}),", valuesComm.Parameters);

                    }
                    else if (valuesClause is PositionedValues)
                    {
                        var valuesComm = valuesClause.GetSqlCommand(keyDefinition);
                        sqlCommand.AppendCommandLine($"\t({valuesComm.Command}),", valuesComm.Parameters);
                    }


                }
                sqlCommand.Command = sqlCommand.Command.TrimEnd($",{Environment.NewLine}".ToCharArray()) + Environment.NewLine;
            }

            if (_onConflict != null) {
                var onConflict = _onConflict.GetSqlCommand(tableDefinition);
                sqlCommand.AppendCommandLine(onConflict.Command, onConflict.Parameters);
            }

            if (_returningClause != null)
            {
                sqlCommand.AppendCommand($"RETURNING {_returningClause.GetSqlCommand(tableDefinition).Command}");
            }


            return sqlCommand;
        }
    }
}
