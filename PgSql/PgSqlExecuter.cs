using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using doob.PgSql.CustomTypes;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.Logging;
using doob.PgSql.TypeMapping;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;
using Reflectensions.ExtensionMethods;

namespace doob.PgSql
{
    public class PgSqlExecuter
    {
        private readonly ILog _logger = LogProvider.For<PgSqlExecuter>();
        private readonly NpgsqlConnection _connection;
        private NpgsqlTransaction _transaction;

        public PgSqlExecuter(ConnectionString connection)
        {
            _connection = new NpgsqlConnection(connection.ToNpgSqlConnectionString());
        }

        public PgSqlExecuter BeginTransaction()
        {
            _connection.OpenConnection();
            _transaction = _connection.BeginTransaction();
            return this;
        }

        public PgSqlExecuter CommitTransaction()
        {
            
            _transaction?.Commit();
            return this;
        }

        public PgSqlExecuter RollbackTransaction()
        {
            _transaction?.Rollback();
            return this;
        }

        public PgSqlExecuter EndTransaction()
        {
            _transaction?.Connection?.CloseConnection();
            _transaction = null;
            _connection?.CloseConnection();
            return this;
        }

        private NpgsqlCommand PrepareCommand(PgSqlCommand sqlCommand)
        {
            var com = new NpgsqlCommand(sqlCommand.Command, _connection);

            _logger.Trace(() => $"PrepareCommand: {com.Connection?.ConnectionString};");

            foreach (var param in sqlCommand.Parameters)
            {
                com.Parameters.Add(BuildNpgsqlParameter(param));
            }

            if (_transaction != null)
            {
                com.Transaction = _transaction;
            }
            com.Connection.OpenConnection();
            return com;
        }
        private async Task<NpgsqlCommand> PrepareCommandAsync(PgSqlCommand sqlCommand)
        {
            var com = new NpgsqlCommand(sqlCommand.Command, _connection);

            _logger.Trace(() => $"PrepareCommand: {com.Connection?.ConnectionString};");

            foreach (var param in sqlCommand.Parameters)
            {
                com.Parameters.Add(BuildNpgsqlParameter(param));
            }

            if (_transaction != null)
            {
                com.Transaction = _transaction;
            }
            await com.Connection.OpenConnectionAsync();
            return com;
        }


        private void EndCommand(NpgsqlCommand sqlCommand) {
            if (_transaction == null) {
                sqlCommand.Connection.CloseConnection();
                sqlCommand.Dispose();
            }
        }

        #region ExecuteNonQuery
        public int ExecuteNonQuery(string sqlCommand)
        {
            var pgSqlCommand = new PgSqlCommand();
            pgSqlCommand.AppendCommand(sqlCommand);
            return ExecuteNonQuery(pgSqlCommand);
        }
        public int ExecuteNonQuery(PgSqlCommand sqlCommand)
        {

            var command = PrepareCommand(sqlCommand);
            return ExecuteNonQuery(command);
        }
        public int ExecuteNonQuery(NpgsqlCommand sqlCommand)
        {

            _logger.Trace(() => $"ExecuteNonQuery::{sqlCommand.CommandText}");

            int result = 0;
            try
            {
                result = sqlCommand.ExecuteNonQuery();
            }
            //catch (PostgresException pgEx)
            //{
            //    if (pgEx.SqlState != "XA0000")
            //        throw;
            //}
            catch (Exception e)
            {

                _logger.ErrorFormat($"ExecuteNonQuery::{{e}}", e);
                throw;
            }
            finally
            {
                EndCommand(sqlCommand);
            }

            return result;
        }

        public Task<int> ExecuteNonQueryAsync(string sqlCommand)
        {
            var pgSqlCommand = new PgSqlCommand();
            pgSqlCommand.AppendCommand(sqlCommand);
            return ExecuteNonQueryAsync(pgSqlCommand);
        }
        public async Task<int> ExecuteNonQueryAsync(PgSqlCommand sqlCommand)
        {
            var command = await PrepareCommandAsync(sqlCommand);
            return await ExecuteNonQueryAsync(command);
        }
        public async Task<int> ExecuteNonQueryAsync(NpgsqlCommand sqlCommand)
        {

            _logger.Trace(() => $"ExecuteNonQuery::{sqlCommand.CommandText}");

            int result;

            try
            {
                result = await sqlCommand.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                _logger.ErrorFormat($"ExecuteNonQuery::{{e}}", e);
                throw;
            }
            finally
            {
                EndCommand(sqlCommand);
            }

            return result;

        }
        #endregion


        #region ExecuteReader
        public IEnumerable<JObject> ExecuteReader(string sqlCommand)
        {
            var pgSqlCommand = new PgSqlCommand();
            pgSqlCommand.AppendCommand(sqlCommand);
            return ExecuteReader(pgSqlCommand);
        }
        public IEnumerable<JObject> ExecuteReader(PgSqlCommand sqlCommand)
        {
            var command = PrepareCommand(sqlCommand);
            return ExecuteReader(command);
        }
        public IEnumerable<JObject> ExecuteReader(NpgsqlCommand sqlCommand)
        {
            _logger.Trace(() => $"ExecuteReader::{sqlCommand.CommandText}");

            List<JObject> items = new List<JObject>();

            try
            {
                using (var reader = sqlCommand.ExecuteReader())
                {
                    var columns = reader.GetColumnSchema();
                    while (reader.Read())
                    {
                        var jo = new JObject();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {

                            jo.Add(columns[i].ColumnName, ConvertFromDb(reader[i], columns[i].DataTypeName));
                        }
                        items.Add(jo);
                    }
                }

            }
            catch (Exception e)
            {
                _logger.Error(e, "ExecuteReader", sqlCommand.CommandText);
                throw;
            }
            finally
            {
                EndCommand(sqlCommand);
            }

            return items;

        }

        public Task<IEnumerable<JObject>> ExecuteReaderAsync(string sqlCommand)
        {
            var pgSqlCommand = new PgSqlCommand();
            pgSqlCommand.AppendCommand(sqlCommand);
            return ExecuteReaderAsync(pgSqlCommand);
        }
        public async Task<IEnumerable<JObject>> ExecuteReaderAsync(PgSqlCommand sqlCommand)
        {
            var command = await PrepareCommandAsync(sqlCommand);
            return await ExecuteReaderAsync(command);
        }
        public async Task<IEnumerable<JObject>> ExecuteReaderAsync(NpgsqlCommand sqlCommand)
        {
            _logger.Trace(() => $"ExecuteReader::{sqlCommand.CommandText}");

            List<JObject> items = new List<JObject>();

            try
            {
                using (var reader = await sqlCommand.ExecuteReaderAsync())
                {

                    var columns = reader.GetColumnSchema();
                    while (await reader.ReadAsync())
                    {
                        var jo = new JObject();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            jo.Add(columns[i].ColumnName, ConvertFromDb(reader[i], columns[i].DataTypeName));
                        }
                        items.Add(jo);
                    }
                }

               
            }
            catch (Exception e)
            {
                _logger.Error(e, "ExecuteReader", sqlCommand.CommandText);
                throw;
            }
            finally
            {
                EndCommand(sqlCommand);
            }

            return items;
        }


        public IEnumerable<T> ExecuteReader<T>(string sqlCommand)
        {
            return ExecuteReader(sqlCommand).Select(Converter.Json.ToObject<T>);
        }
        public IEnumerable<T> ExecuteReader<T>(PgSqlCommand sqlCommand)
        {
            return ExecuteReader(sqlCommand).Select(Converter.Json.ToObject<T>);
        }
        public IEnumerable<T> ExecuteReader<T>(NpgsqlCommand sqlCommand)
        {
            return ExecuteReader(sqlCommand).Select(Converter.Json.ToObject<T>);
        }

        public Task<IEnumerable<T>> ExecuteReaderAsync<T>(string sqlCommand)
        {
            return ExecuteReaderAsync(sqlCommand).SelectAsync(Converter.Json.ToObject<T>);
        }
        public Task<IEnumerable<T>> ExecuteReaderAsync<T>(PgSqlCommand sqlCommand)
        {
            return ExecuteReaderAsync(sqlCommand).SelectAsync(Converter.Json.ToObject<T>);
        }
        public Task<IEnumerable<T>> ExecuteReaderAsync<T>(NpgsqlCommand sqlCommand)
        {
            return ExecuteReaderAsync(sqlCommand).SelectAsync(Converter.Json.ToObject<T>);
        }
        #endregion


        #region ExecuteScalar
        public TOut ExecuteScalar<TOut>(string sqlCommand)
        {

            var pgSqlCommand = new PgSqlCommand();
            pgSqlCommand.AppendCommand(sqlCommand);
            return ExecuteScalar<TOut>(pgSqlCommand);
        }
        public TOut ExecuteScalar<TOut>(PgSqlCommand sqlCommand)
        {
            var command = PrepareCommand(sqlCommand);
            return ExecuteScalar<TOut>(command);
        }
        public TOut ExecuteScalar<TOut>(NpgsqlCommand sqlCommand)
        {
            if (sqlCommand == null) throw new ArgumentNullException(nameof(sqlCommand));

            _logger.Trace(() => $"ExecuteScalar<{typeof(TOut).FullName}>::{sqlCommand.CommandText}");

            TOut result;
            try
            {
                result = sqlCommand.ExecuteScalar().CloneTo<TOut>();
            }
            catch (Exception e)
            {
                _logger.DebugFormat($"ExecuteScalar<{typeof(TOut).FullName}>::{{command}}", sqlCommand.CommandText);
                _logger.DebugFormat($"ExecuteScalar<{typeof(TOut).FullName}>::{{e}}", e);
                throw;
            }
            finally
            {
                EndCommand(sqlCommand);
            }

            return result;
        }


        public Task<TOut> ExecuteScalarAsync<TOut>(string sqlCommand)
        {

            var pgSqlCommand = new PgSqlCommand();
            pgSqlCommand.AppendCommand(sqlCommand);
            return ExecuteScalarAsync<TOut>(pgSqlCommand);
        }
        public async Task<TOut> ExecuteScalarAsync<TOut>(PgSqlCommand sqlCommand)
        {
            var command = await PrepareCommandAsync(sqlCommand);
            return await ExecuteScalarAsync<TOut>(command);
        }
        public async Task<TOut> ExecuteScalarAsync<TOut>(NpgsqlCommand sqlCommand)
        {

            _logger.Trace(() => $"ExecuteScalar<{typeof(TOut).FullName}>::{sqlCommand.CommandText}");

            TOut result;

            try
            {
                result = await sqlCommand.ExecuteScalarAsync().CastToTaskOf<TOut>();
            }
            catch (Exception e)
            {
                _logger.DebugFormat($"ExecuteScalar<{typeof(TOut).FullName}>::{{command}}", sqlCommand.CommandText);
                _logger.DebugFormat($"ExecuteScalar<{typeof(TOut).FullName}>::{{e}}", e);
                throw;
            } finally {
                EndCommand(sqlCommand);
            }

            return result;
        }


        public object ExecuteScalar(string sqlCommand)
        {
            return ExecuteScalar<object>(sqlCommand);
        }
        public object ExecuteScalar(PgSqlCommand sqlCommand)
        {
            return ExecuteScalar<object>(sqlCommand);
        }
        public object ExecuteScalar(NpgsqlCommand sqlCommand)
        {
            return ExecuteScalar<object>(sqlCommand);
        }

        public Task<object> ExecuteScalarAsync(string sqlCommand)
        {
            return ExecuteScalarAsync<object>(sqlCommand);
        }
        public Task<object> ExecuteScalarAsync(PgSqlCommand sqlCommand)
        {
            return ExecuteScalarAsync<object>(sqlCommand);
        }
        public Task<object> ExecuteScalarAsync(NpgsqlCommand sqlCommand)
        {
            return ExecuteScalarAsync<object>(sqlCommand);
        }
        #endregion


        public static JToken ConvertFromDb(object data, string pgTypeName)
        {

            if (data == null || data == DBNull.Value)
                return null;

            var npgsqlType = PgSqlTypeManager.Global.GetNpgsqlDbType(pgTypeName);

            JToken newObject = null;
            switch (npgsqlType)
            {
                case NpgsqlDbType.Json | NpgsqlDbType.Array:
                case NpgsqlDbType.Jsonb | NpgsqlDbType.Array:
                {
                    var list = new JArray();
                    var enumerable = data as IEnumerable;
                    foreach (var o in enumerable)
                    {
                        var jt = JToken.Parse(o.ToString());

                        list.Add(jt);
                    }
                    newObject = list;
                    break;
                }
                case NpgsqlDbType.Json:
                case NpgsqlDbType.Jsonb:
                {
                    newObject = JToken.Parse(data.ToString());
                    break;
                }
                default:
                    newObject = JToken.FromObject(data);
                    break;
            }
            return newObject;
        }
        public static NpgsqlParameter BuildNpgsqlParameter(PgSqlParameter param)
        {
            try
            {
                if (param.Value == null)
                    return new NpgsqlParameter(param.UniqueId, DBNull.Value);

                NpgsqlDbType type = NpgsqlDbType.Text;

                

                if (param.Column?.DotNetType?.IsEnum == true || param.Column?.DotNetType.IsNullableType() == true && param.Column?.DotNetType.GetInnerTypeFromNullable().IsEnum == true)
                {
                    switch (param.Value)
                    {
                        case int i:
                        {
                            var val = (Enum)Enum.ToObject(param.Column.DotNetType, i);
                            return new NpgsqlParameter(param.UniqueId, NpgsqlDbType.Text) { Value = val.GetName() };
                        }
                        case Enum @enum:
                        {
                            return new NpgsqlParameter(param.UniqueId, NpgsqlDbType.Text) { Value = @enum.GetName() };
                        }
                    }
                }

                if (!String.IsNullOrWhiteSpace(param.OverrideType))
                {
                    type = PgSqlTypeManager.Global.GetNpgsqlDbType(param.OverrideType);
                }
                else
                {
                    type = param.Column?.GetNpgSqlDbType() ?? PgSqlTypeManager.Global.GetNpgsqlDbType(param.ClrType);
                }

                //if (!Enum.TryParse(param.OverrideType, true, out type))
                //{
                //    type = param.Column?.GetNpgSqlDbType() ?? NpgsqlDbType.Jsonb;
                //}

                if (type == NpgsqlDbType.Json || type == NpgsqlDbType.Jsonb)
                {
                    var valueType = PgSqlTypeManager.Global.GetNpgsqlDbType(param.Value.GetType());
                    if (valueType != NpgsqlDbType.Json && valueType != NpgsqlDbType.Jsonb)
                        type = NpgsqlDbType.Text;
                }

                if(param.Value is PgSqlLTree ltree)
                {
                    return new NpgsqlParameter(param.UniqueId, NpgsqlDbType.Unknown) { Value = ltree.ToString() };
                }


                object value = null;
                switch (type)
                {
                    case NpgsqlDbType.Array | NpgsqlDbType.Json:
                    case NpgsqlDbType.Array | NpgsqlDbType.Jsonb: {
                        var objAr = new List<object>();
                        if (param.Value is string)
                        {
                            objAr.Add(param.Value.ToString());
                        }
                        else
                        {
                            if (param.Value is IEnumerable arr) {
                                foreach (var o in arr) {
                                    objAr.Add(Converter.Json.ToJson(o));
                                }
                            }
                                
                        }

                        value = objAr;
                        break;
                    }
                    case NpgsqlDbType.Json:
                    case NpgsqlDbType.Jsonb: {
                        value = Converter.Json.ToJson(param.Value);
                        break;
                    }
                    case NpgsqlDbType.Array | NpgsqlDbType.Uuid: {
                        var json = Converter.Json.ToJson(param.Value);
                        value = Converter.Json.ToObject<List<Guid>>(json);

                        break;
                    }
                    case NpgsqlDbType.Uuid: {
                        value = new Guid(param.Value.ToString());
                        break;
                    }
                    default: {
                        
                        value = param.Value;
                        break;
                    }
                }


                return new NpgsqlParameter(param.UniqueId, type) { Value = value };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

    }
}
