using System;
using System.Collections.Generic;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.TypeMapping;

namespace doob.PgSql
{
    public class ColumnBuilder
    {
        public Column Column;

        public ColumnBuilder() : this(new Column())
        {

        }

        internal ColumnBuilder(Column column)
        {
            Column = column;
        }

        private int _currentTablePosition { get; set; }

        public ColumnBuilder SetPosition(int? position)
        {
            Column.Position = position;
            return this;
        }

        public ColumnBuilder SetClrName(string name)
        {
            Column.ClrName = name;
            return this;
        }
        public ColumnBuilder SetDbName(string name)
        {
            Column.DbName = name;
            return this;
        }


        public ColumnBuilder CanBeNull()
        {
            return Nullable(true);
        }
        public ColumnBuilder CanNotBeNull()
        {
            return Nullable(false);
        }
        public ColumnBuilder Nullable(bool value)
        {
            Column.CanBeNull = value;
            return this;
        }

        public ColumnBuilder MustBeUnique()
        {
            return MustBeUnique(true);
        }
        public ColumnBuilder MustBeUnique(bool value)
        {
            Column.MustBeUnique = true;
            return this;
        }

        public ColumnBuilder AsPrimaryKey()
        {
            return AsPrimaryKey(true);
        }
        public ColumnBuilder AsPrimaryKey(bool value)
        {
            Column.IsPrimaryKey = value;
            return this;
        }

        public ColumnBuilder DefaultValue(string value)
        {
            Column.DefaultValue = value;
            return this;
        }


        public ColumnBuilder SetCustomDbType(string value)
        {
            Column.PgType = value;
            return this;
        }

        internal ColumnBuilder SetTablePosition(int position)
        {
            _currentTablePosition = position;
            return this;
        }
        internal int GetTablePosition()
        {
            return _currentTablePosition;
        }


        public static ColumnBuilder Build(string name, Type dotnetType)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            name = name.ClearString();
            var col = new ColumnBuilder().SetClrName(name);

            col.Column.DotNetType = dotnetType ?? throw new ArgumentNullException(nameof(dotnetType));
            return col;
        }
        public static ColumnBuilder Build(string name, string typeName)
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
        public static ColumnBuilder Build<T>(string name)
        {
            return Build(name, typeof(T));
        }




        public static implicit operator Column(ColumnBuilder builder)
        {
            return builder?.Column;
        }
    }
}