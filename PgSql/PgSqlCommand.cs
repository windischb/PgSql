using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace doob.PgSql
{
    public class PgSqlCommand
    {
        private StringBuilder _commandBuilder = new StringBuilder();
        public string Command
        {
            get { return _commandBuilder.ToString().Trim(); }
            set { _commandBuilder = new StringBuilder(value); }
        }

        public List<PgSqlParameter> Parameters { get; } = new List<PgSqlParameter>();

        public PgSqlCommand AppendCommand(string command)
        {
            _commandBuilder.Append(command);
            return this;
        }
        public PgSqlCommand AppendCommand(string command, IEnumerable<PgSqlParameter> items)
        {
            _commandBuilder.Append(prepareAddCommand(command, items));
            return this;
        }
        public PgSqlCommand AppendCommandLine(string command)
        {
            _commandBuilder.AppendLine(command);
            return this;
        }
        public PgSqlCommand AppendCommandLine(string command, IEnumerable<PgSqlParameter> items)
        {
            _commandBuilder.AppendLine(prepareAddCommand(command, items));
            return this;
        }
        

        public void SetValue(string columnName, string parameterName, object value)
        {
            Parameters.RemoveAll(p => p.ParameterName.Equals(parameterName, StringComparison.OrdinalIgnoreCase));

            var qe = new PgSqlParameter(columnName, value, parameterName);

            Parameters.Add(qe);
        }
        public void OverrideValueType(string columnName, string parameterName, string overrideType)
        {
            foreach (var pgSqlParameter in Parameters.Where(p => p.ParameterName.Equals(parameterName, StringComparison.OrdinalIgnoreCase) && p.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)))
            {
                pgSqlParameter.OverrideType = overrideType;
            }
        }
        internal bool ParameterIdExists(string id)
        {
            return Parameters.Exists(p => p.UniqueId.Equals(id, StringComparison.OrdinalIgnoreCase));
        }
        private string prepareAddCommand(string command, IEnumerable<PgSqlParameter> parameters)
        {
            foreach (var param in parameters)
            {
                if (Parameters.Any(par => par.UniqueId.Equals(param.UniqueId)))
                {
                    var newParam = param.RebuildWithNewId();
                    command = command.Replace($"@{param.UniqueId}", $"@{newParam.UniqueId}");
                    Parameters.Add(newParam);
                }
                else
                {
                    Parameters.Add(param);
                }
            }

            return command;
        }

        public string CommandAsPlainText()
        {

            var str = Command;
            foreach (var expressionValueItem in Parameters)
            {
                JToken jToken;
                if (expressionValueItem.Value == null)
                {
                    jToken = JValue.CreateNull();
                }
                else
                {
                    jToken = Converter.Json.ToJToken(expressionValueItem.Value);
                }

                string value = null;
                switch (jToken.Type)
                {
                    case JTokenType.Object:
                    {
                        value = $"'{jToken.ToString(Formatting.None)}'";
                        break;
                    }
                    case JTokenType.Float:
                    case JTokenType.Integer:
                    case JTokenType.Boolean:
                    {
                        value = jToken.ToObject<string>();
                        break;
                    }

                    case JTokenType.String:
                    {
                        var valstr = jToken.ToObject<string>();
                        if (valstr.Equals("default", StringComparison.OrdinalIgnoreCase))
                        {
                            value = "DEFAULT";
                        }
                        else
                        {
                            value = $"'{jToken.ToObject<string>()}'";
                        }

                        break;
                    }
                    case JTokenType.Array:
                    {
                        value = $"'{jToken.ToString()}'";
                        break;
                    }
                    case JTokenType.Null:
                    {
                        value = "null";
                        break;
                    }
                    default:
                    {
                        value = $"'{jToken.ToObject<string>()}'";
                        break;
                    }
                }

                str = str.Replace($"@{expressionValueItem.UniqueId}", value);
            }

            return str;
        }

    }
}
