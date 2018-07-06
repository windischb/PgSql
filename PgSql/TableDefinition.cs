using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using doob.PgSql.Attributes;
using doob.PgSql.Exceptions;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.Logging;
using doob.PgSql.Tables;
using doob.PgSql.TypeMapping;

namespace doob.PgSql
{

    public class TableDefinition
    {

        private static readonly ILog Logger = LogProvider.For<TableDefinition>();
        private OrderedColumnsList OrderedColumns { get; } = new OrderedColumnsList();

        public TableDefinition() { }
        private TableDefinition(OrderedColumnsList orderedColumnsList)
        {
            OrderedColumns = orderedColumnsList.Clone();
        }



        public Column[] PrimaryKeys()
        {
            return OrderedColumns.GetOrderedColums().Where(c => c.IsPrimaryKey).ToArray();
        }
        public Column[] Columns()
        {
            var nLst = OrderedColumns.GetOrderedColums();
            return nLst.ToArray();
        }

        public ColumnBuilder AddColumn(string name, string typeName)
        {
            Column col = ColumnBuilder.Build(name, typeName);
            AddColumn(col);
            return GetColumnBuilderByClrName(col.ClrName);
        }


        public TableDefinition AddColumn(Column column)
        {
            if (Columns().Any(c => c.ClrName != null  && c.ClrName == column.ClrName))
                throw new ColumnAlreadyExistsException();

            if (Columns().Any(c => c.DbName != null && c.DbName == column.DbName))
                throw new ColumnAlreadyExistsException();

            OrderedColumns.Add(column);

            return this;
        }
        public TableDefinition AddColumn(IEnumerable<Column> columns)
        {
            foreach (var column in columns)
            {
                AddColumn(column);
            }
            return this;
        }


        public ColumnBuilder GetColumnBuilderByClrName(string name)
        {
            try
            {
                if (name.Contains("."))
                    name = name.Split('.')[0];

                var column = Columns().FirstOrDefault(c => c.ClrName?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

                return column?.Builder();
            }
            catch (Exception e)
            {
                Logger.Error(e, $"GetColumn('{name}') from {{Columns}}", new object[] { Columns() });
                throw;
            }
        }

        public ColumnBuilder GetColumnBuilderByDbName(string name)
        {
            try
            {
                if (name.Contains("."))
                    name = name.Split('.')[0];

                var column = Columns().FirstOrDefault(c => c.DbName?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

                return column?.Builder();
            }
            catch (Exception e)
            {
                Logger.Error(e, $"GetColumn('{name}') from {{Columns}}", new object[] { Columns() });
                throw;
            }
        }

        public Column GetColumnByClrName(string name)
        {
            return GetColumnBuilderByClrName(name);
        }

        public Column GetColumnByDbName(string name)
        {
            return GetColumnBuilderByDbName(name);
        }



        public TableDefinition Clone()
        {
            return new TableDefinition(OrderedColumns);
        }

        public TableDefinition ClearPrimaryKeys()
        {
            foreach (var pgSqlColumn in OrderedColumns.GetOrderedColums())
            {
                pgSqlColumn.IsPrimaryKey = false;
            }
            return this;
        }

        public TableDefinition ClearDefaultValues()
        {
            foreach (var pgSqlColumn in OrderedColumns.GetOrderedColums())
            {
                pgSqlColumn.DefaultValue = null;
            }
            return this;
        }

        public TableDefinition ClearUnique()
        {
            foreach (var pgSqlColumn in OrderedColumns.GetOrderedColums())
            {
                pgSqlColumn.MustBeUnique = false;
            }
            return this;
        }


        public static TableDefinition FromType(Type type)
        {
            return FromType<object>(type);
        }

        public static TableDefinition<T> FromType<T>()
        {
            return FromType<T>(null);
        }

        private static TableDefinition<T> FromType<T>(Type type)
        {
            if (type == null)
                type = typeof(T);

            var def = new TableDefinition<T>();
            var props = type
                .GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public |
                               BindingFlags.NonPublic)
                .Where(p => !p.IsDefined(typeof(PgSqlIgnoreAttribute)));

            foreach (var propertyInfo in props)
            {
                if (def.GetColumnBuilderByClrName(propertyInfo.Name) != null)
                    continue;

                if (propertyInfo.GetGetMethod(true)?.IsPublic == false &&
                    !propertyInfo.IsDefined(typeof(PgSqlIncludeAttribute)))
                    continue;

                ColumnBuilder col = ColumnBuilder.Build(propertyInfo.Name, propertyInfo.PropertyType);


                var attributes = propertyInfo.GetCustomAttributes<PgSqlAttribute>(true);
                foreach (var attr in attributes) {

                    if (attr is PgSqlPrimaryKeyAttribute primaryKeyAttribute) {
                        col.AsPrimaryKey();
                        if (!String.IsNullOrWhiteSpace(primaryKeyAttribute.Value?.ToString())) {
                            col.DefaultValue(primaryKeyAttribute.Value?.ToString());
                        }
                    }

                    if (attr is PgSqlColumnAttribute colAttr)
                        col.SetDbName(colAttr.Name);
                    
                    if (attr is PgSqlDefaultValueAttribute defaultvalue)
                        col.DefaultValue(defaultvalue.Value?.ToString());

                    if (attr is PgSqlUniqueAttribute unique)
                        col.MustBeUnique((bool)unique.Value);

                    if (attr is PgSqlCustomTypeAttribute custom)
                        col.SetCustomDbType((string)custom.Value);
                }



                def.AddColumn(col);
            }

            return def;
        }

        public static TableDefinition FromTable(ITable table)
        {
            return FromTable<object>(table);
        }

        public static TableDefinition<T> FromTable<T>(TypedTable<T> table)
        {
            return FromTable<T>((ITable)table);
        }

        internal static TableDefinition<T> FromTable<T>(ITable table)
        {
            var td = TableDefinition.FromType<T>();
            try
            {
                var columns = new DbExecuter(table.GetConnectionString()).ExecuteReader(SQLStatements.GetColumns(table.GetConnectionString().TableName, table.GetConnectionString().SchemaName)).ToArray();
                foreach (var column in columns)
                {

                    Column dbCol = column.ToObject<Column>();

                    var col = td.GetColumnByDbName(dbCol.DbName) ?? td.GetColumnByClrName(dbCol.DbName);

                    if (col == null)
                    {
                        dbCol.DotNetType = PgSqlTypeManager.GetDotNetType(dbCol.PgType);
                        td.AddColumn(dbCol);

                    }
                    else
                    {
                        col.DbName = dbCol.DbName;
                        col.CanBeNull = dbCol.CanBeNull;
                        col.DefaultValue = dbCol.DefaultValue;
                        col.Position = dbCol.Position;
                        col.IsPrimaryKey = dbCol.IsPrimaryKey;
                        col.MustBeUnique = dbCol.MustBeUnique;
                    }

                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "GetTableSchema");
                throw;
            }

            return td;
        }


    }

    public class TableDefinition<T> : TableDefinition
    {

        public ColumnBuilder GetColumnBuilder(Expression<Func<T, object>> field)
        {
            var name = field.GetPropertyName();
            return base.GetColumnBuilderByClrName(name);
        }

    }

}