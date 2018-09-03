using System;
using System.Collections.Generic;
using System.Text;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.Tables;
using Reflectensions.ExtensionMethods;

namespace doob.PgSql
{
    public static class ConnectTo
    {

        #region Server
        public static Server Server()
        {
            return Server(ConnectionString.Build());
        }
        public static Server Server(string connectionString)
        {
            return Server(ConnectionString.Build(connectionString));
        }
        public static Server Server(Action<ConnectionStringBuilder> builder)
        {
            return Server(builder.InvokeAction());
        }
        public static Server Server(ConnectionStringBuilder connectionBuilder)
        {
            return Server(connectionBuilder.GetConnection());
        }
        public static Server Server(ConnectionString connectionString)
        {
            return new Server(connectionString);
        }
        #endregion


        #region Database
        public static Database Database()
        {
            return Database(ConnectionString.Build());
        }
        public static Database Database(string connectionString)
        {
            return Database(ConnectionString.Build(connectionString));
        }
        public static Database Database(Action<ConnectionStringBuilder> builder)
        {
            return Database(builder.InvokeAction());
        }
        public static Database Database(ConnectionStringBuilder connectionStringBuilder)
        {
            return Database(connectionStringBuilder.GetConnection());
        }
        public static Database Database(ConnectionString connectionString)
        {
            return new Database(connectionString);
        }
        #endregion


        #region Schema
        public static Schema Schema()
        {
            return Schema(ConnectionString.Build());
        }
        public static Schema Schema(string connectionString)
        {
            return Schema(ConnectionString.Build(connectionString));
        }
        public static Schema Schema(Action<ConnectionStringBuilder> builder)
        {
            return Schema(builder.InvokeAction());
        }
        public static Schema Schema(ConnectionStringBuilder connectionStringBuilder)
        {
            return Schema(connectionStringBuilder.GetConnection());
        }
        public static Schema Schema(ConnectionString connectionString)
        {
            return new Schema(connectionString);
        }
        #endregion


        #region TypedTable
        public static TypedTable<T> Table<T>(string connectionString)
        {
            return Table<T>(ConnectionString.Build(connectionString));
        }
        public static TypedTable<T> Table<T>(Action<ConnectionStringBuilder> builder)
        {
            return Table<T>(builder.InvokeAction());
        }
        public static TypedTable<T> Table<T>(ConnectionStringBuilder connectionStringBuilder)
        {
            return Table<T>(connectionStringBuilder.GetConnection());
        }
        public static TypedTable<T> Table<T>(ConnectionString connectionString)
        {
            return new TypedTable<T>(connectionString);
        }

        public static TypedTable<T> Table<T>(string connectionString, TableDefinition<T> tableDefinition)
        {
            return Table<T>(ConnectionString.Build(connectionString), tableDefinition);
        }
        public static TypedTable<T> Table<T>(Action<ConnectionStringBuilder> builder, TableDefinition<T> tableDefinition)
        {
            return Table<T>(builder.InvokeAction(), tableDefinition);
        }
        public static TypedTable<T> Table<T>(ConnectionStringBuilder connectionStringBuilder, TableDefinition<T> tableDefinition)
        {
            return Table<T>(connectionStringBuilder.GetConnection(), tableDefinition);
        }
        public static TypedTable<T> Table<T>(ConnectionString connectionString, TableDefinition<T> tableDefinition)
        {
            return new TypedTable<T>(connectionString, tableDefinition);
        }
        #endregion


        #region ObjectTable
        public static ObjectTable Table(string connectionString)
        {
            return Table(ConnectionString.Build(connectionString));
        }
        public static ObjectTable Table(Action<ConnectionStringBuilder> builder)
        {
            return Table(builder.InvokeAction());
        }
        public static ObjectTable Table(ConnectionStringBuilder connectionStringBuilder)
        {
            return Table(connectionStringBuilder.GetConnection());
        }
        public static ObjectTable Table(ConnectionString connectionString)
        {
            return new ObjectTable(connectionString);
        }

        public static ObjectTable Table(string connectionString, TableDefinition tableDefinition)
        {
            return Table(ConnectionString.Build(connectionString), tableDefinition);
        }
        public static ObjectTable Table(Action<ConnectionStringBuilder> builder, TableDefinition tableDefinition)
        {
            return Table(builder.InvokeAction(), tableDefinition);
        }
        public static ObjectTable Table(ConnectionStringBuilder connectionStringBuilder, TableDefinition tableDefinition)
        {
            return Table(connectionStringBuilder.GetConnection(), tableDefinition);
        }
        public static ObjectTable Table(ConnectionString connectionString, TableDefinition tableDefinition)
        {
            return new ObjectTable(connectionString, tableDefinition);
        }
        #endregion


    }

    public static class Build
    {

        #region ConnectionString
        public static ConnectionString ConnectionString(string connectionString)
        {
            return new ConnectionString(connectionString);
        }
        public static ConnectionString ConnectionString(ConnectionStringBuilder connectionStringBuilder)
        {
            return connectionStringBuilder.GetConnection();
        }
        public static ConnectionString ConnectionString(Action<ConnectionStringBuilder> builder)
        {
            return ConnectionString(builder.InvokeAction());
        }
        #endregion

        #region ConnectionStringBuilder
        public static ConnectionStringBuilder ConnectionStringBuilder()
        {
            return new ConnectionStringBuilder();
        }
        public static ConnectionStringBuilder ConnectionStringBuilder(string connectionString)
        {
            return new ConnectionStringBuilder(connectionString);
        }
        public static ConnectionStringBuilder ConnectionStringBuilder(ConnectionString connectionString)
        {
            return new ConnectionStringBuilder(connectionString);
        }
        #endregion

        #region TableDefinition
        public static TableDefinition TableDefinition()
        {
            return new PgSql.TableDefinition();
        }
        public static TableDefinition TableDefinition(Type fromType)
        {
            return PgSql.TableDefinition.FromType<object>(fromType);
        }
        public static TableDefinition<T> TableDefinition<T>()
        {
            return PgSql.TableDefinition.FromType<T>(null);
        }
        public static TableDefinition TableDefinition(ITable table)
        {
            return PgSql.TableDefinition.FromTable<object>(table);
        }
        public static TableDefinition<T> TableDefinition<T>(TypedTable<T> table)
        {
            return PgSql.TableDefinition.FromTable<T>((ITable)table);
        }
        #endregion
    }
}
