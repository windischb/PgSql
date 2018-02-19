using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using doob.PgSql.Interfaces;

namespace doob.PgSql.Clauses
{
    public class Set : IUpdateMember
    {
        private readonly List<IValueItem> _values = new List<IValueItem>();

        internal Set() { }

        public static Set Value(string key, object value)
        {
            return new Set().SetValue(key, value);
        }

        public Set SetValue(string key, object value)
        {
            try
            {


                if (value != null)
                {


                    if (IsHashSet(value))
                    {
                        var gen = value.GetType().GenericTypeArguments[0];
                        var z = typeof(List<>).MakeGenericType(gen);
                        var l = (IList)Activator.CreateInstance(z);
                        var col = value as IEnumerable;
                        if (col != null)
                            foreach (var o in col)
                            {
                                l.Add(o);
                            }

                        value = l;
                    }

                }
                var exists = _values.FirstOrDefault(v => v._key.Equals(key, StringComparison.OrdinalIgnoreCase));
                if (exists == null)
                {
                    var val = new ValueItem(key, value);
                    _values.Add(val);
                }
                else
                {
                    exists._value = value;
                }
                return this;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        internal bool IsHashSet(object obj)
        {
            if (obj != null)
            {
                var t = obj.GetType();
                if (t.GetTypeInfo().IsGenericType)
                {
                    return t.GetGenericTypeDefinition() == typeof(HashSet<>);
                }
            }
            return false;
        }


        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();

            _values.ForEach(v =>
            {
                if (tableDefinition == null || tableDefinition.GetColumn(v._key) != null)
                {
                    var param = new PgSqlParameter(v._key, v._value).SetColum(tableDefinition);
                    sqlCommand.AppendCommandLine($"\"{param.ColumnName}\" = @{param.UniqueId},", new List<PgSqlParameter>() { param });
                }
            });


            sqlCommand.Command = sqlCommand.Command.TrimEnd($",{Environment.NewLine}".ToCharArray());

            return sqlCommand;
        }
    }
}
