using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using doob.PgSql.Clauses;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.Helper;
using doob.PgSql.Interfaces.Where;
using doob.PgSql.Listener;
using doob.PgSql.Logging;
using doob.PgSql.Statements;
using doob.PgSql.TypeMapping;
using Newtonsoft.Json.Linq;
using Npgsql;
using Npgsql.Schema;

namespace doob.PgSql.Tables
{
    public abstract class BaseTable<TTable, TItem> : ITable where TTable : BaseTable<TTable, TItem>
    {

        private static readonly ILog Logger = LogProvider.For<TTable>();

        private readonly ConnectionString _connectionString;

        //protected Lazy<TableDefinition<TItem>> LazyTableDefinition => new Lazy<TableDefinition<TItem>>(() =>
        //{
        //    return PgSql.TableDefinition.FromTable<TItem>(this);
        //});

        private object _tdLock = new object();
        private TableDefinition<TItem> _tableDefinition;
        protected TableDefinition<TItem> GetTableDefinition(bool force = false)
        {
            if (force) {
                return (_tableDefinition = TableDefinition.FromTable<TItem>(this));
            }

            if (_tableDefinition == null) {
                lock (_tdLock) {
                    return _tableDefinition ?? (_tableDefinition = TableDefinition.FromTable<TItem>(this));
                }
            }

            return _tableDefinition;
        }
        

        public TableDefinition TableDefinition => GetTableDefinition();

        private DbExecuter _dbExecuter;

        protected SecureDataManager SecureDataManager;

        #region Constructor

        protected BaseTable() : this(ConnectionString.Build()) { }
        protected BaseTable(string connectionstring) : this(ConnectionString.Build(connectionstring)) { }
        protected BaseTable(ConnectionStringBuilder connectionBuilder) : this(connectionBuilder.GetConnection()) { }
        protected BaseTable(Action<ConnectionStringBuilder> connectionBuilder) : this(connectionBuilder.InvokeAction()) { }
        protected BaseTable(ConnectionString connection)
        {

            if (String.IsNullOrWhiteSpace(connection.TableName))
                throw new ArgumentException(nameof(connection.TableName));

            _connectionString = ConnectionString.Build(connection);

        }

        #endregion



        protected DbExecuter Execute()
        {
            return _dbExecuter ?? (_dbExecuter = new DbExecuter(_connectionString));
        }


        public ConnectionString GetConnectionString() => _connectionString.Clone();

        public Server GetServer()
        {
            return new Server(_connectionString);
        }
        public Database GetDatabase()
        {
            return new Database(_connectionString);
        }
        public Schema GetSchema()
        {
            return new Schema(_connectionString);
        }


        #region Check Exists

        public bool Exists()
        {
            return GetDatabase().SchemaExists(_connectionString.SchemaName);
        }
        public virtual TTable EnsureExists()
        {
            GetSchema().EnsureExists().TableExists(_connectionString.TableName, true, false);
            return (TTable)this;
        }
        public virtual TTable EnsureNotExists()
        {
            GetSchema().TableExists(_connectionString.TableName, false, true);
            return (TTable)this;
        }

        #endregion


        public Schema Drop(bool throwIfNotExists = false)
        {
            return GetSchema().TableDrop(_connectionString.TableName, throwIfNotExists);
        }

        protected TTable ConfigureCertificateEncryption(string certificatePath, string password)
        {
            SecureDataManager = new SecureDataManager(certificatePath, password);
            return (TTable)this;
        }


        private TableListener _onNotification;
        public TableListener Notifications(NotificationSharedBy sharedBy = NotificationSharedBy.Schema) => _onNotification ?? (_onNotification = DMLTriggerManager.GetTiggerListener(this, sharedBy));


        #region Triggers

        public bool TriggerExists(string triggerName)
        {
            return Execute().ExecuteScalar<bool>(SQLStatements.TableTriggerExists(triggerName, this));
        }

        public void TriggerDrop(string triggerName)
        {
            Execute().ExecuteNonQuery(SQLStatements.TableTriggerDrop(triggerName, this));
        }

        public void TriggerCreate(string triggerName, string triggerFunctionName, bool overwriteIfExists = false)
        {
            var triggerExists = TriggerExists(triggerName);

            if (!triggerExists || overwriteIfExists)
            {
                if (triggerExists)
                    TriggerDrop(triggerName);

                Execute().ExecuteNonQuery(SQLStatements.TableTriggerCreate(triggerName, triggerFunctionName, this));
            }
        }

        #endregion



        public TypedTable<TOut> Typed<TOut>()
        {
            var tbl = new TypedTable<TOut>(_connectionString)
            {
                SecureDataManager = SecureDataManager
            };
            return tbl;

        }
        public ObjectTable NotTyped()
        {
            var tbl = new ObjectTable(_connectionString)
            {
                SecureDataManager = SecureDataManager
            };
            return tbl;
        }

        protected string GetTableName()
        {
            return $"\"{_connectionString.SchemaName.Trim()}\".\"{ _connectionString.TableName.Trim()}\"".Trim('.');
        }

        public TTable BeginTransaction()
        {
            var tbl = (TTable)Activator.CreateInstance(typeof(TTable), _connectionString);
            tbl.Execute().BeginTransaction();
            return tbl;
        }

        public TTable CommitTransaction()
        {
            Execute().CommitTransaction();
            return (TTable)this;
        }

        public TTable RollbackTransaction()
        {
            Execute().RollbackTransaction();
            return (TTable)this;
        }

        public TTable EndTransaction()
        {
            Execute().EndTransaction();
            return (TTable)this;
        }



        #region Query

        protected JObject[] Query(Select select)
        {

            if (select._from == null)
            {
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

        protected JObject[] Query(string sqlQuery)
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

        protected JObject QueryByPrimaryKey(object value)
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

                var pKeyName = prNames.FirstOrDefault()?.DbName;
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

        protected Dictionary<string, object> Insert(object document)
        {
            return Insert(document, null);
        }

        private Dictionary<string, object> BuildInsertData(object document)
        {
            if (document == null)
                return null;

            var jo = JObject.FromObject(document);
            var dict = new Dictionary<string, object>();
            foreach (var col in TableDefinition.Columns())
            {
                var colName = col.GetNameForDb();
                if (!col.CanBeNull && !String.IsNullOrWhiteSpace(col.DefaultValue))
                {
                    if (jo.ContainsKey(colName))
                    {
                        var val = jo[colName];

                        if (val.Type == JTokenType.Null)
                            continue;

                        if ((val.Type == JTokenType.Integer || val.Type == JTokenType.Float) && Math.Abs(val.ToObject<float>() - 0) < 0)
                            continue;

                        dict.Add(colName, val.ToObject(col.DotNetType));
                    }
                }

            }

            return dict;
        }


        protected Dictionary<string, object> Insert(object document, List<string> returnValues)
        {
            var dict = document.ToDotNetDictionary();
            foreach (var col in TableDefinition.Columns())
            {

                if (!col.CanBeNull && !String.IsNullOrWhiteSpace(col.DefaultValue))
                {
                    if (dict.ContainsKey(col.GetNameForDb()))
                    {
                        if (dict[col.GetNameForDb()] == null)
                        {
                            dict.Remove(col.GetNameForDb());
                            continue;
                        }

                        if (dict[col.GetNameForDb()] is long l)
                        {
                            if (l == 0)
                            {
                                dict.Remove(col.GetNameForDb());
                                continue;
                            }

                        }
                    }
                }



            }

            var insert = Statements.Insert.Into(GetTableName())
                .AddColumnsFromTableDefinition(TableDefinition)
                .AddValuesFromObject(dict);


            if (returnValues == null || !returnValues.Any())
            {
                returnValues = TableDefinition.PrimaryKeys().Select(p => p.DbName).ToList();
            }

            if (returnValues.Any())
                insert.AddClause(Returning.Columns(returnValues.ToArray()));

            return Execute().ExecuteReader<Dictionary<string, object>>(insert.GetSqlCommand(TableDefinition)).FirstOrDefault();
        }

        protected List<Dictionary<string, object>> Insert<TAny>(IEnumerable<TAny> documents)
        {
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
                    if (!col.CanBeNull && !String.IsNullOrWhiteSpace(col.DefaultValue))
                    {
                        if (dict.ContainsKey(col.ClrName))
                        {
                            if (dict[col.ClrName] == null)
                            {
                                dict.Remove(col.ClrName);
                            }
                        }
                    }

                }
                insert.AddValuesFromObject(dict);
            }

            if (returnValues == null || !returnValues.Any())
            {
                returnValues = TableDefinition.PrimaryKeys().Select(p => p.DbName).ToList();
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




        public HistoryTable History()
        {
            return new HistoryTable(this);
        }


        public override string ToString()
        {
            return $"\"{_connectionString.SchemaName}\".\"{_connectionString.TableName}\"";
        }

    }

}