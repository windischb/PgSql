using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using doob.PgSql.Clauses;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.Interfaces;
using doob.PgSql.Interfaces.Where;

namespace doob.PgSql.Statements
{
    public class Update : ISQLCommand
    {
        private IUpdateDestination _updateDestination;
        private Set _set;
        private IWhere _whereClause;

        public Update(string table)
        {
            _updateDestination = new UpdateDestinationTable(table);
        }
        public Update(string table, string alias)
        {
            _updateDestination = new UpdateDestinationTable(table, alias);
        }

        public static Update Table(string name)
        {
            return new Update(name);
        }
        public static Update Table(string name, string alias)
        {
            return new Update(name, alias);
        }

        public Update SetValue(string key, object value)
        {
            if (_set == null)
                _set = new Set();

            _set.SetValue(key, value);
            return this;
        }

        public Update SetValues(params KeyValuePair<string, object>[] values)
        {
            if (_set == null)
                _set = new Set();

            foreach (var o in values)
            {
                _set.SetValue(o.Key, o.Value);
            }
            return this;
        }

        public Update SetValueFromObject(object @object)
        {

            
            var nDict = @object.ToColumsDictionary(); // Converter.Json.ToDictionary(@object);

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

            //        tempDict = @object.GetType().GetProperties().ToDictionary(
            //            (p) => p.Name,
            //            (p) => p.GetValue(@object),
            //            StringComparer.OrdinalIgnoreCase);
            //    }
            //}

            return SetValues(nDict.ToArray());
        }


        //public Update SetValueFromObject(object @object, TableDefinition tableDefinition)
        //{

        //    Dictionary<string, object> tempDict = null;
        //    if (@object is JObject)
        //        tempDict = ((JObject)@object).ToObject<Dictionary<string, object>>();

        //    if (tempDict == null)
        //    {
        //        var dict = @object as IDictionary<string, object>;
        //        if (dict != null)
        //        {
        //            tempDict = new Dictionary<string, object>(dict, StringComparer.OrdinalIgnoreCase);
        //        }
        //        else
        //        {
        //            tempDict = @object.ToDotNetDictionary();
        //        }
        //    }

        //    foreach (var keyValuePair in tempDict)
        //    {
        //        object val = keyValuePair.Value;
        //        SetValue(keyValuePair.Key, val);
        //    }

        //    return this;
        //}

        public Update Where(IWhere whereClause)
        {
            _whereClause = whereClause;
            return this;
        }

        public Update AddClause(IUpdateMember clause)
        {
            switch (clause)
            {
                case Set set:
                    _set = set;
                    break;
                case IWhere where:
                    _whereClause = where;
                    break;
            }

            return this;
        }

        public PgSqlCommand GetSqlCommand()
        {
            return GetSqlCommand(null);
        }

       
        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();


            sqlCommand.AppendCommandLine($"UPDATE {_updateDestination.GetSqlCommand(tableDefinition).Command}");

            if (_set != null)
            {
                sqlCommand.AppendCommandLine("SET");
                var setComm = _set.GetSqlCommand(tableDefinition);
                sqlCommand.AppendCommand($"{AddIntend(setComm.Command, 4)}", setComm.Parameters);
            }

            if (_whereClause != null)
            {
                var whereComm = _whereClause.GetSqlCommand(tableDefinition);
                sqlCommand.AppendCommandLine($"WHERE {whereComm.Command}", whereComm.Parameters);
            }

            return sqlCommand;
        }

        private string AddIntend(string text, int level)
        {
            var lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            var indent = new string(' ', level);
            var strBuilder = new StringBuilder();
            foreach (var line in lines)
            {
                strBuilder.AppendLine($"{indent}{line}");
            }
            return strBuilder.ToString();
        }
    }
}
