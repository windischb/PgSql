using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using doob.PgSql.Clauses;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.Helper;
using doob.PgSql.Interfaces.Where;
using doob.PgSql.Logging;
using doob.PgSql.Statements;
using doob.PgSql.TypeMapping;
using Npgsql;
using Npgsql.Schema;

namespace doob.PgSql.Tables
{
    public abstract class BaseTable<T, TItem> where T : BaseTable<T, TItem>
    {

        private static readonly ILog Logger = LogProvider.For<BaseTable<T, TItem>>();

        private ConnectionString _connectionString;
        public ConnectionString ConnectionString
        {
            get
            {
                if (_connectionString == null)
                    _connectionString = ConnectionString.Build();

                return _connectionString;
            }
        }

        private Lazy<TableDefinition> _lazyTableDefinition;

        protected TableDefinition TableDefinition => _lazyTableDefinition?.Value;

        protected internal SecureDataManager _secureDataManager;

        private DbExecuter _dbExecuter;

        private DbExecuter Execute()
        {
            if (_dbExecuter == null)
                _dbExecuter = new DbExecuter(ConnectionString);

            return _dbExecuter;
        }

        protected BaseTable() : this(ConnectionString.Build()) { }
        protected BaseTable(string connectionstring) : this(ConnectionString.Build(connectionstring)) { }
        protected BaseTable(ConnectionString connection) : this(ConnectionString.Build(connection)) { }
        protected BaseTable(ConnectionStringBuilder connectionBuilder)
        {
            var con = connectionBuilder.GetConnection();
            if (String.IsNullOrWhiteSpace(con.TableName))
                throw new ArgumentException(nameof(con.TableName));

            _connectionString = ConnectionString.Build(connectionBuilder);
            _lazyTableDefinition = new Lazy<TableDefinition>(UpdateTableDefinition);
        }

        public Server GetServer()
        {
            return new Server(ConnectionString);
        }
        public Database GetDatabase()
        {
            return new Database(ConnectionString);
        }
        public Schema GetSchema()
        {
            return new Schema(ConnectionString);
        }

        public bool Exists()
        {
            return GetDatabase().SchemaExists(ConnectionString.SchemaName);
        }
        public T EnsureExists()
        {
            GetSchema().TableExists(ConnectionString.TableName, true, false);
            return (T)this;
        }
        public T EnsureNotExists()
        {
            GetSchema().TableExists(ConnectionString.TableName, false, true);
            return (T)this;
        }

        public void Drop()
        {
            GetSchema().TableDrop(ConnectionString.TableName);
        }
        public void Drop(bool throwIfNotExists)
        {
            GetSchema().TableDrop(ConnectionString.TableName, throwIfNotExists);
        }

        protected T ConfigureCertificateEncryption(string certificatePath, string password)
        {
            _secureDataManager = new SecureDataManager(certificatePath, password);
            return (T)this;
        }

        public TableDefinition GetTableDefinition()
        {
            return GetTableDefinition(false);
        }
        public TableDefinition GetTableDefinition(bool force)
        {
            if (_lazyTableDefinition == null || TableDefinition == null || force)
            {
                _lazyTableDefinition = new Lazy<TableDefinition>(UpdateTableDefinition);
            }
            return TableDefinition;
        }

        protected TableDefinition UpdateTableDefinition()
        {

            var td = new TableDefinition();

            var tabl = GetTableSchema().ToList();

            foreach (var str in tabl)
            {
                //var domainName = str.DataTypeName;
                //var postgresType = str.DataTypeName;
                //if (domainName.StartsWith("secstring"))
                //{
                //    postgresType = "text";
                //}

                //var name = str.ColumnName;
                ////var postgresType = str.DataTypeName;
                //var position = str.ColumnOrdinal.ToInt();
                //var @default = str.DefaultValue;
                //var nullable = str.AllowDBNull.CastToBoolean();
                //var isPrimaryKey = str.IsKey.CastToBoolean();


                //var dataType = str.DataType;
                //if (dataType == typeof(Array))
                //    postgresType = $"{postgresType.TrimStart('_')}[]";

                //var type = str.DataType;
                //var col = Column.Build(name, type)
                //    .SetPosition(position)
                //    .DefaultValue(@default)
                //    .AsPrimaryKey(isPrimaryKey)
                //    .Nullable(nullable);



                td.AddColumn(str);
            }
            return td;
        }


        protected Column GetColumn(int position)
        {
            return TableDefinition.Columns()?.FirstOrDefault(c => c.Properties.Position == position);
        }

        protected Column GetColumn(string name)
        {
            return
                TableDefinition.Columns()?.FirstOrDefault(
                    c => String.Equals(c.Properties.Name, name.Trim("\"".ToCharArray()), StringComparison.CurrentCultureIgnoreCase));
        }



        #region Triggers

        public bool EventTriggerExists(string triggerName)
        {
            var command = $"SELECT EXISTS(select 1 from pg_trigger where tgname = '{triggerName}');";
            return new DbExecuter(ConnectionString).ExecuteScalar<bool>(command);
        }

        public void EventTriggerDrop(string triggerName, bool force)
        {
            var command = $"DROP EVENT TRIGGER IF EXISTS \"{triggerName}\"";

            if (force)
                command = $"{command} CASCADE";

            new DbExecuter(ConnectionString).ExecuteNonQuery(command);
        }


        public void RegisterEventTrigger(ListenMode listenMode, bool overwriteIfExists = false)
        {

            string listenFunctionName;
            var functioncommand = "";
            switch (listenMode)
            {
                case ListenMode.HistoryTableId:
                case ListenMode.HistoryTableEntry:
                    HistoryTable.EnsureExists();
                    listenFunctionName = $"XA-Notify-TableEvent_ByHistory";
                    functioncommand = TableStatements.BuildTriggerFunction_ByHistoryKeys(ConnectionString.SchemaName, listenFunctionName, listenMode);
                    break;
                case ListenMode.ReferenceTablePrimaryKeys:
                case ListenMode.ReferenceTableEntry:
                    listenFunctionName = $"XA-Notify-TableEvent_ByReference";
                    functioncommand = TableStatements.BuildTriggerFunction_ByReferenceKeys(ConnectionString.SchemaName, listenFunctionName, listenMode);
                    break;
                default:
                    throw new NotImplementedException(listenMode.ToString());
            }

            if (!GetSchema().FunctionExists(listenFunctionName) || overwriteIfExists)
            {
                new DbExecuter(ConnectionString).ExecuteNonQuery(functioncommand);
            }

            var trigger1Exists = EventTriggerExists($"{listenFunctionName}_Trigger::{ConnectionString.SchemaName}.{ConnectionString.TableName}");

            if (!trigger1Exists || overwriteIfExists)
            {
                if (trigger1Exists)
                    EventTriggerDrop($"{listenFunctionName}_Trigger::{ConnectionString.SchemaName}.{ConnectionString.TableName}", true);

                var trigger1 = $"CREATE TRIGGER \"{listenFunctionName}_Trigger::{ConnectionString.SchemaName}.{ConnectionString.TableName}\" AFTER INSERT OR UPDATE OR DELETE ON \"{ConnectionString.SchemaName}\".\"{ConnectionString.TableName}\" FOR EACH ROW EXECUTE PROCEDURE \"{ConnectionString.SchemaName}\".\"{listenFunctionName}\"();";
                new DbExecuter(ConnectionString).ExecuteNonQuery(trigger1);
            }

        }

        #endregion

        private HistoryTable<T, TItem> _histTable;
        internal HistoryTable<T, TItem> HistoryTable
        {
            get
            {
                if (_histTable == null)
                    _histTable = new HistoryTable<T, TItem>(this);

                return _histTable;
            }
        }

        public TypedTable<TOut> Typed<TOut>()
        {
            var tbl = new TypedTable<TOut>(ConnectionString)
            {
                _secureDataManager = _secureDataManager
            };
            return tbl;

        }
        public ObjectTable NotTyped()
        {
            var tbl = new ObjectTable(this.ConnectionString.Clone())
            {
                _secureDataManager = _secureDataManager
            };
            return tbl;
        }

        protected string GetTableName()
        {
            return $"\"{ConnectionString.SchemaName.Trim()}\".\"{ ConnectionString.TableName.Trim()}\"".Trim('.');
        }

        public T BeginTransaction()
        {
            var tbl = (T)Activator.CreateInstance(typeof(T), this.ConnectionString);
            tbl.Execute().BeginTransaction();
            return tbl;
        }

        public T CommitTransaction()
        {
            Execute().CommitTransaction();
            return (T)this;
        }

        public T RollbackTransaction()
        {
            Execute().RollbackTransaction();
            return (T)this;
        }

        public T EndTransaction()
        {
            Execute().EndTransaction();
            return (T)this;
        }


        #region Query

        protected string[] Query(Select select)
        {

            if (select._from == null) {
                select.From(From.TableOrView(GetTableName()));
            }

            try
            {
                return Execute().ExecuteReader(select.GetSqlCommand(TableDefinition)).ToArray();
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Query: {select.GetSqlCommand(TableDefinition).CommandAsPlainText()}");
                throw;
            }

        }

        protected string[] Query(string sqlQuery)
        {
            try
            {
                return Execute().ExecuteReader(sqlQuery).ToArray();
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Query(Raw): {sqlQuery}");
                throw;
            }

        }

        //private string QueryByPrimaryKey(Dictionary<string, object> value)
        //{
        //    var prNames = TableDefinition.PrimaryKeys();

        //    var from = From.TableOrView(GetTableName());

        //    var where = new List<IWhere>();
        //    foreach (var prName in prNames)
        //    {
        //        if (value.ContainsKey(prName.Name))
        //            where.Add(Where.Create().Eq(prName.Name, value[prName.Name]));
        //    }

        //    var selectStatement = new Select().AddColumnsFromTableDefinition(TableDefinition)
        //        .From(from)
        //        .Where(Where.MergeQueriesAND(where));

        //    return Query(selectStatement).FirstOrDefault();
        //}


        protected string QueryByPrimaryKey(object value)
        {
            var prNames = TableDefinition.PrimaryKeys();
            Dictionary<string, object> dict = new Dictionary<string, object>();
            if (value.IsDictionaryType() || value.CanConvertToDictionary())
            {
                dict = value.ToDotNetDictionary();
            }
            else
            {
                if (prNames.Length > 1)
                    throw new Exception("Table has more than one PrimaryKey, please provide a Dictionary with Key/Value");

                var pKeyName = prNames.FirstOrDefault().Properties.Name;
                dict.Add(pKeyName, value);
            }

            
            var from = From.TableOrView(GetTableName());

            var where = new List<IWhere>();
            
            foreach (var kv in dict)
            {
                where.Add(Where.Create().Eq(kv.Key, kv.Value));
            }

            //var where = Where.Create().Eq(prName.Name, value);
            var selectStatement = new Select().AddColumnsFromTableDefinition(TableDefinition)
                .From(from)
                .Where(Where.MergeQueriesAND(where));

            return Query(selectStatement).FirstOrDefault();
        }




        #endregion

        #region Insert

        protected Dictionary<string, object> Insert(object document) {
            return Insert(document, null);
        }
        protected Dictionary<string, object> Insert(object document, List<string> returnValues)
        {
            var dict = document.ToDotNetDictionary();
            foreach (var col in TableDefinition.Columns())
            {

                if (!col.Properties.Nullable && !String.IsNullOrWhiteSpace(col.Properties.DefaultValue))
                {
                    if (dict.ContainsKey(col.Properties.Name))
                    {
                        if (dict[col.Properties.Name] == null)
                        {
                            dict.Remove(col.Properties.Name);
                            continue;
                        }

                        if (dict[col.Properties.Name] is long l)
                        {
                            if (l == 0)
                            {
                                dict.Remove(col.Properties.Name);
                                continue;
                            }

                        }
                    }
                }

                

            }

            var insert = Statements.Insert.Into(GetTableName())
                .AddColumnsFromTableDefinition(TableDefinition)
                .AddValuesFromObject(dict);

            
            if (returnValues == null || !returnValues.Any()) {
                returnValues = TableDefinition.PrimaryKeys().Select(p => p.Properties.Name).ToList();
            }

            if(returnValues.Any())
                insert.AddClause(Returning.Columns(returnValues.ToArray()));
            
            return Execute().ExecuteReader<Dictionary<string, object>>(insert.GetSqlCommand(TableDefinition)).FirstOrDefault();
        }

        protected List<Dictionary<string, object>> Insert<TAny>(IEnumerable<TAny> documents) {
            return Insert<TAny>(documents, null);
        }
        protected List<Dictionary<string, object>> Insert<TAny>(IEnumerable<TAny> documents, List<string> returnValues)
        {
            var insert = Statements.Insert.Into(GetTableName())
                .AddColumnsFromTableDefinition(TableDefinition);
            foreach (var document in documents)
            {
                var dict = document.ToDotNetDictionary();
                foreach (var col in TableDefinition.Columns())
                {
                    if (!col.Properties.Nullable && !String.IsNullOrWhiteSpace(col.Properties.DefaultValue))
                    {
                        if (dict.ContainsKey(col.Properties.Name))
                        {
                            if (dict[col.Properties.Name] == null)
                            {
                                dict.Remove(col.Properties.Name);
                            }
                        }
                    }

                }
                insert.AddValuesFromObject(dict);
            }

            if (returnValues == null || !returnValues.Any())
            {
                returnValues = TableDefinition.PrimaryKeys().Select(p => p.Properties.Name).ToList();
            }

            if (returnValues.Any())
                insert.AddClause(Returning.Columns(returnValues.ToArray()));

            return Execute().ExecuteReader<Dictionary<string, object>>(insert.GetSqlCommand(TableDefinition)).ToList();
        }

        #endregion

        #region Update
        protected object Update(IWhere query, object document)
        {
            var update = Statements.Update.Table(GetTableName()).SetValueFromObject(document).Where(query);

            Logger.Debug(update.GetSqlCommand(TableDefinition).CommandAsPlainText);

            try
            {
                return Execute().ExecuteScalar(update.GetSqlCommand(TableDefinition));
            }
            catch (Exception e)
            {
                Logger.Error(e, "Update", update.GetSqlCommand(TableDefinition).CommandAsPlainText());
                throw;
            }

        }
        #endregion

        #region Delete

        protected int Delete(IWhere query)
        {
            var from = From.TableOrView(GetTableName());
            var delStatement = Statements.Delete.From(from).Where(query);
            try
            {
                return Execute().ExecuteNonQuery(delStatement.GetSqlCommand(TableDefinition));
            }
            catch (Exception e)
            {
                Logger.Error(e, "Delete", delStatement.GetSqlCommand(TableDefinition).CommandAsPlainText());
                throw;
            }

        }


        #endregion

        #region Exists

        protected bool Exists(IWhere where, NpgsqlConnection npgsqlConnection)
        {
            var from = From.TableOrView(GetTableName());
            var sel = new Select().From(from).Where(where);

            var exists = Select.Exists(new Exists(sel));


            try
            {
                return Execute().ExecuteScalar<bool>(exists.GetSqlCommand(TableDefinition));
            }
            catch (Exception e)
            {
                Logger.Error(e, "Exists", exists.GetSqlCommand(TableDefinition).CommandAsPlainText());
                throw;
            }

        }

        protected bool Exists(string field, object value, NpgsqlConnection npgsqlConnection)
        {
            var q = Where.Create().Eq(field, value);
            return Exists(q, npgsqlConnection);
        }

        #endregion

        //#region Execute

        //public int ExecuteNonQuery(string sql) {
        //    try
        //    {
        //        return Execute().ExecuteNonQuery(sql);
        //    }
        //    catch (Exception e)
        //    {
        //        Logger.Error(e, "ExecuteNonQuery", sql);
        //        throw;
        //    }
        //}

        //public IEnumerable<T> ExecuteReader<T>(string sql) {
        //    return Execute().ExecuteReader<T>(sql);
        //}

        //#endregion


        protected Column[] GetTableSchema()
        {
            var schema = new List<Column>();
            try {
                var columns = Execute().ExecuteReader(SQLStatements.GetColumns(_connectionString.TableName, _connectionString.SchemaName)).ToArray();
                foreach (var column in columns) {
                    var dict = JSON.ToDictionary(column, true);
                    var col = new Column();
                    col.Properties.Name = dict["column_name"].ToString();
                    col.Properties.Nullable = dict["is_nullable"].CastToBoolean();
                    col.Properties.DefaultValue = dict["column_default"]?.ToString();
                    col.Properties.Position = dict["ordinal_position"].ToInt() -1;
                    col.Properties.PrimaryKey = dict["isprimarykey"].CastToBoolean();
                    col.Properties.Unique = dict["isunique"].CastToBoolean();

                    var dbType = dict["udt_name"].ToString().Trim();
                    var dataType = dict["data_type"].ToString();
                    if (dataType.Equals("ARRAY"))
                        dbType = $"{dbType}[]";

                    col.Properties.DotNetType = PgSqlTypeManager.GetDotNetType(dbType);
                    schema.Add(col);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "GetTableSchema");
                throw;
            }
            return schema.ToArray();
        }

        protected NpgsqlDbColumn[] GetTableSchema2()
        {
            var schema = new List<NpgsqlDbColumn>();
            try
            {
                var sql = $"Select * from {GetTableName()} LIMIT 1";
                var con = new DbExecuter(ConnectionString).BuildNpgSqlConnetion();

                using (NpgsqlCommand command = new NpgsqlCommand(sql, con.OpenConnection()))
                {
                    using (var reader = command.ExecuteReader(CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo))
                    {
                        var columns = reader.GetColumnSchema();
                        schema.AddRange(columns);
                    }
                }
                con.CloseConnection();
            }
            catch (Exception e)
            {
                Logger.Error(e, "GetTableSchema");
                throw;
            }
            return schema.ToArray();
        }



        public override string ToString() {
            return $"\"{ConnectionString.SchemaName}\".\"{ConnectionString.TableName}\"";
        }

    }

}