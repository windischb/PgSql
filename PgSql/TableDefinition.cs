using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using doob.PgSql.Attributes;
using doob.PgSql.Exceptions;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.Logging;

namespace doob.PgSql {

    public class TableDefinition {

        private static readonly ILog Logger = LogProvider.For<TableDefinition>();

        public OrderedColumnsList OrderedColumns { get; set; } = new OrderedColumnsList();

        public TableDefinition() { }

        private TableDefinition(OrderedColumnsList orderedColumnsList) {
            OrderedColumns = orderedColumnsList.Clone();
        }

        public Column[] PrimaryKeys() {
            return OrderedColumns.GetOrderedColums().Where(c => c.IsPrimaryKey).ToArray();
        }

        public Column[] Columns() {
            var nLst = OrderedColumns.GetOrderedColums();
            return nLst.ToArray();
        }

        public TableDefinition AddColumn(Column column) {
            if (Columns().Any(c => c.Name == column.Name))
                throw new ColumnAlreadyExistsException();

            OrderedColumns.Add(column);

            return this;
        }

        public ColumnBuilder AddColumn(string name, string typeName) {
            Column col = ColumnBuilder.Build(name, typeName);
            AddColumn(col);
            return GetColumn(col.Name);
        }



        public ColumnBuilder GetColumn(string name) {
            try {
                if (name.Contains("."))
                    name = name.Split('.')[0];

                var column = Columns().FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if(column == null)
                    column = Columns().FirstOrDefault(c => c.Alias?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

                return column?.Builder();
            } catch (Exception e) {
                Logger.Error(e, $"GetColumn('{name}') from {{Columns}}", new object[] {Columns()});
                throw;
            }
        }


        public TableDefinition Clone() {
            return new TableDefinition(OrderedColumns);
        }

        public static TableDefinition FromType(Type type) {
            return FromType<object>(type);
        }

        public static TableDefinition<T> FromType<T>() {
            return FromType<T>(null);
        }

        private static TableDefinition<T> FromType<T>(Type type) {
            if (type == null)
                type = typeof(T);

            var def = new TableDefinition<T>();
            var props = type
                .GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public |
                               BindingFlags.NonPublic)
                .Where(p => !p.IsDefined(typeof(PgSqlIgnoreAttribute)));

            foreach (var propertyInfo in props) {
                if (def.GetColumn(propertyInfo.Name) != null)
                    continue;

                if (propertyInfo.GetGetMethod(true)?.IsPublic == false &&
                    !propertyInfo.IsDefined(typeof(PgSqlIncludeAttribute)))
                    continue;


               
               
                ColumnBuilder col = ColumnBuilder.Build(propertyInfo.Name, propertyInfo.PropertyType);

                //if (propertyInfo.PropertyType.IsEnum)
                //    col.SetCustomDbType("pg_enum_type");

                var attributes = propertyInfo.GetCustomAttributes<PgSqlAttribute>(true);
                foreach (var attr in attributes) {

                    if (attr is PgSqlPrimaryKeyAttribute primaryKeyAttribute) {
                        col.AsPrimaryKey();
                        if (!String.IsNullOrWhiteSpace(primaryKeyAttribute.Value?.ToString())) {
                            col.DefaultValue(primaryKeyAttribute.Value?.ToString());
                        }
                    }

                    if (attr is PgSqlColumnAttribute colAttr) {
                        col.SetName(colAttr.Name);
                    }

                    if (attr is PgSqlAliasAttribute alias) {
                        col.SetAlias(alias.Value.ToString());
                    }
                    
                    if (attr is PgSqlDefaultValueAttribute defaultvalue)
                        col.DefaultValue(defaultvalue.Value?.ToString());

                    if (attr is PgSqlUniqueAttribute unique)
                        col.MustBeUnique((bool) unique.Value);

                    if (attr is PgSqlCustomTypeAttribute custom)
                        col.SetCustomDbType((string) custom.Value);
                }


                
                def.AddColumn(col);
            }

            return def;
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

    }

    public class TableDefinition<T> : TableDefinition {

        public ColumnBuilder GetColumn(Expression<Func<T, object>> field) {
            var name = field.GetPropertyName();
            return base.GetColumn(name);
        }

    }

}