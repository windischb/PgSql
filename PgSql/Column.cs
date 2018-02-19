using System;
using System.Collections.Generic;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.TypeMapping;
using Newtonsoft.Json;

namespace doob.PgSql
{
    public class Column
    {
        public ColumnProperties Properties { get; set; } = new ColumnProperties();

        private int _currentTablePosition { get; set; }

        public Column SetPosition(int? position)
        {
            Properties.Position = position;
            return this;
        }

        public Column SetName(string name)
        {
            Properties.Name = name;
            return this;
        }

        public Column CanBeNull()
        {
            return Nullable(true) ;
        }
        public Column CanNotBeNull() {
            return Nullable(false);
        }
        public Column Nullable(bool value)
        {
            Properties.Nullable = value;
            return this;
        }

        public Column MustBeUnique()
        {
            return MustBeUnique(true);
        }
        public Column MustBeUnique(bool value)
        {
            Properties.Unique = true;
            return this;
        }

        public Column AsPrimaryKey()
        {
            return AsPrimaryKey(true);
        }
        public Column AsPrimaryKey(bool value)
        {
            Properties.PrimaryKey = value;
            return this;
        }

        public Column DefaultValue(string value)
        {
            Properties.DefaultValue = value;
            return this;
        }

        
        public static Column Build(string name, Type dotnetType)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            name = name.ClearString();
            var col = new Column().SetName(name);

            col.Properties.DotNetType = dotnetType ?? throw new ArgumentNullException(nameof(dotnetType));
            return col;
        }
        public static Column Build(string name, string typeName)
        {
            Type type = null;
            string tName = typeName;
            bool isArray = false;
            if (tName.EndsWith("[]"))
            {
                isArray = true;
                tName = tName.TrimEnd("[]".ToCharArray());
            }


            type = Type.GetType(tName, false, true);

            if (type == null)
                type = Type.GetType($"System.{tName}", false, true);

            if (type == null)
                type = PgSqlTypeManager.GetDotNetType(tName);

            if (type == null)
                throw new Exception($"Can't find Type '{typeName}'");

            if (isArray)
            {
                type = typeof(List<>).MakeGenericType(type);
            }

            var col = Build(name, type);
            return col;
        }
        public static Column Build<T>(string name) {
            return Build(name, typeof(T));
        }

        internal Column SetTablePosition(int position)
        {
            _currentTablePosition = position;
            return this;
        }
        internal int GetTablePosition()
        {
            return _currentTablePosition;
        }
    }

    public class ColumnProperties {

        [JsonProperty]
        public int? Position { get; internal set; }

        [JsonProperty]
        public string Name { get; internal set; }
        [JsonProperty]
        public string Alias { get; internal set; }
        [JsonProperty]
        public bool Nullable { get; internal set; } = true;
        [JsonProperty]
        public bool Unique { get; internal set; }
        [JsonProperty]
        public bool PrimaryKey { get; internal set; }
        [JsonProperty]
        public string DefaultValue { get; internal set; }
        [JsonProperty]
        public string CustomDbType { get; set; }
        [JsonProperty]
        public Type DotNetType { get; internal set; }

    }
}
