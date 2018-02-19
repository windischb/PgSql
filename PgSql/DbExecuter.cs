using System;
using System.Collections.Generic;
using System.Linq;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.Logging;
using Npgsql;

namespace doob.PgSql
{
    public class DbExecuter
    {
        private static readonly ILog Logger = LogProvider.For<DbExecuter>();
        private NpgsqlConnection _connection;
        private NpgsqlTransaction _transaction;
        private readonly object _lock = new object();

        public DbExecuter()
        {
            _connection = new NpgsqlConnection(new ConnectionString().ToNpgSqlConnectionString());
        }
        public DbExecuter(string connectionString)
        {
            _connection = new NpgsqlConnection(new ConnectionString(connectionString).ToNpgSqlConnectionString());
        }
        public DbExecuter(ConnectionString connection)
        {
            _connection = new NpgsqlConnection(connection.ToNpgSqlConnectionString());
        }
        public DbExecuter(ConnectionStringBuilder connectionBuilder)
        {
            _connection = new NpgsqlConnection(connectionBuilder.GetConnection().ToNpgSqlConnectionString());
        }

        private DbExecuter ReUseNpgSqlConnection(NpgsqlConnection connection)
        {
            //if (connection != null)
            //    _connection = connection;

            return this;
        }
        
        public DbExecuter BeginTransaction()
        {
            _connection.OpenConnection();
            _transaction = _connection.BeginTransaction();
            return this;
        }


        public DbExecuter CommitTransaction()
        {
            _transaction?.Commit();
            return this;
        }

        public DbExecuter RollbackTransaction()
        {
            _transaction?.Rollback();
            return this;
        }

        public DbExecuter EndTransaction()
        {
            if (_transaction == null)
                return this;

            _transaction = null;
            _connection.CloseConnection();
            return this;
        }

        private NpgsqlCommand PrepareCommand(string cmdText)
        {
            var conn = new NpgsqlCommand(cmdText, _connection);

            Logger.Debug(() => $"PrepareCommand: {conn.Connection.ConnectionString};");

            if (_transaction != null)
            {
                conn.Transaction = _transaction;
            }
            conn.Connection.OpenConnection();
            return conn;
        }

        private void EndCommand(NpgsqlCommand command)
        {
            if (_transaction == null)
            {
                command.Connection.CloseConnection();
            }
        }

        public int ExecuteNonQuery(string sqlStatement)
        {
            var sqlCommand = new PgSqlCommand();
            sqlCommand.AppendCommand(sqlStatement);
            return ExecuteNonQuery(sqlCommand);
        }
        public int ExecuteNonQuery(PgSqlCommand sqlCommand)
        {

            Logger.Debug(() =>
            {
                return $"ExecuteNonQuery::{sqlCommand.CommandAsPlainText()}";
            });

            try
            {
                lock (_lock)
                {
                    int ret;
                    var command = PrepareCommand(sqlCommand.Command);

                    foreach (var param in sqlCommand.Parameters)
                    {
                        command.Parameters.Add(param.ToNpgsqlParameter());
                    }
                    ret = command.ExecuteNonQuery();

                    EndCommand(command);
                    return ret;
                }
            }
            catch (Exception e)
            {
                Logger.ErrorFormat($"ExecuteNonQuery::{{e}}", e);
                throw;
            }
            
        }
        

        public IEnumerable<T> ExecuteReader<T>(string sqlStatement)
        {
            return ExecuteReader(sqlStatement).Select(JSON.ToObject<T>);
        }
        public IEnumerable<string> ExecuteReader(string sqlStatement)
        {
            var sqlCommand = new PgSqlCommand();
            sqlCommand.AppendCommand(sqlStatement);
            return ExecuteReader(sqlCommand);
        }


        public IEnumerable<T> ExecuteReader<T>(PgSqlCommand sqlCommand)
        {
            return ExecuteReader(sqlCommand).Select(JSON.ToObject<T>);
        }
        public IEnumerable<string> ExecuteReader(PgSqlCommand sqlCommand)
        {
            Logger.Debug(() =>
            {
                return $"ExecuteReader::{sqlCommand.CommandAsPlainText()}";
            });

            try
            {
                lock (_lock)
                {
                    List<string> items = new List<string>();

                    //Console.WriteLine($"##### - {sqlCommand.CommandAsPlainText()}");
                    var command = PrepareCommand(sqlCommand.Command);


                    foreach (var sqlCommandParameter in sqlCommand.Parameters)
                    {
                        var p = sqlCommandParameter.ToNpgsqlParameter();
                        command.Parameters.Add(p);
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        var columns = reader.GetColumnSchema();
                        while (reader.Read())
                        {
                            var item = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++) {
                                item[columns[i].ColumnName] = DbExecuterHelper.ConvertFromDB(reader[i], columns[i]);
                            }
                            items.Add(JSON.ToJson(item));
                        }
                    }
                    EndCommand(command);

                    return items;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "ExecuteReader", sqlCommand.CommandAsPlainText());
                throw;
            }

        }



        public object ExecuteScalar(string sqlStatement)
        {
            return ExecuteScalar<object>(sqlStatement);
        }
        public object ExecuteScalar(PgSqlCommand sqlCommand)
        {
            return ExecuteScalar<object>(sqlCommand);
        }
        public TOut ExecuteScalar<TOut>(string sqlStatement)
        {

            var sqlCommand = new PgSqlCommand();
            sqlCommand.AppendCommand(sqlStatement);
            return ExecuteScalar<TOut>(sqlCommand);
        }
        public TOut ExecuteScalar<TOut>(PgSqlCommand sqlCommand)
        {


            Logger.Debug(() =>
            {
                return $"ExecuteScalar<{typeof(TOut).FullName}>::{sqlCommand.CommandAsPlainText()}";
            });
            TOut ret = default(TOut);

            try
            {
                lock (_lock)
                {
                    var command = PrepareCommand(sqlCommand.Command);

                    foreach (var npgsqlParameter in sqlCommand.Parameters)
                    {
                        command.Parameters.Add(npgsqlParameter.ToNpgsqlParameter());
                    }

                    ret = (TOut) command.ExecuteScalar();

                    EndCommand(command);
                }
            }
            catch (Exception e)
            {
                Logger.ErrorFormat($"ExecuteScalar<{typeof(TOut).FullName}>::{{command}}", sqlCommand.CommandAsPlainText());
                Logger.ErrorFormat($"ExecuteScalar<{typeof(TOut).FullName}>::{{e}}", e);
            }
            return ret;
        }



        private DbExecuter OpenConnection()
        {
            _connection.OpenConnection();
            return this;
        }

        private DbExecuter CloseConnection()
        {
            _connection.CloseConnection();
            return this;
        }

        internal NpgsqlConnection BuildNpgSqlConnetion()
        {
            return _connection;
        }
    }
}
