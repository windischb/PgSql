using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using doob.PgSql.Clauses;
using doob.PgSql.CustomTypes;
using doob.PgSql.Exceptions;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.Helper;
using doob.PgSql.Interfaces;
using doob.PgSql.Interfaces.Where;
using doob.PgSql.Interfaces.Where.NotTyped;
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

        private readonly object _tdLock = new object();

        protected TableDefinition<TItem> _tableDefinition;


        protected TableDefinition<TItem> GetPostgresTableDefinition(bool force = false)
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
        

        public TableDefinition PostgresTableDefinition => GetPostgresTableDefinition();

        private PgSqlExecuter _pgSqlExecuter;

        protected SecureDataManager SecureDataManager;

        #region Constructor

        protected BaseTable() : this(ConnectionString.Build()) { }
        protected BaseTable(string connectionstring) : this(ConnectionString.Build(connectionstring)) { }
        protected BaseTable(ConnectionStringBuilder connectionBuilder) : this(connectionBuilder.GetConnection()) { }
        protected BaseTable(Action<ConnectionStringBuilder> connectionBuilder) : this(connectionBuilder.InvokeAction()) { }
        protected BaseTable(ConnectionString connection)
        {
            var missing = new List<string>();

            if(String.IsNullOrWhiteSpace(connection.DatabaseName))
                missing.Add(nameof(connection.DatabaseName));

            if (String.IsNullOrWhiteSpace(connection.SchemaName))
                missing.Add(nameof(connection.SchemaName));

            if (String.IsNullOrWhiteSpace(connection.TableName))
                missing.Add(nameof(connection.TableName));

            if(missing.Any())
                throw new ArgumentNullException($"Missing Options to connect: '{String.Join(", ", missing)}'");

           _connectionString = ConnectionString.Build(connection);

        }

        #endregion

        


        protected PgSqlExecuter Execute()
        {
            return _pgSqlExecuter ?? new PgSqlExecuter(_connectionString);
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



        protected internal TypedTable<TOut> Typed<TOut>()
        {
            var tbl = new TypedTable<TOut>(_connectionString)
            {
                SecureDataManager = SecureDataManager
            };
            return tbl;

        }
        protected internal ObjectTable NotTyped()
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
            this._pgSqlExecuter = Execute();
            this._pgSqlExecuter?.BeginTransaction();
            return (TTable)this;
        }

        public TTable CommitTransaction()
        {
            this._pgSqlExecuter?.CommitTransaction();
            return (TTable)this;
        }

        public TTable RollbackTransaction()
        {
            this._pgSqlExecuter?.RollbackTransaction();
            return (TTable)this;
        }

        public TTable EndTransaction()
        {
            this._pgSqlExecuter?.EndTransaction();
            this._pgSqlExecuter = null;
            return (TTable)this;
        }


        protected object _createTable(TableDefinition tableDefinition, bool throwIfAlreadyExists)
        {
           
            var pd = GetDatabase();

            foreach (var column in tableDefinition.Columns())
            {
                if (column.DotNetType == typeof(Guid?) || column.DotNetType == typeof(Guid))
                {
                    if (!pd.ExtensionExists("uuid-ossp"))
                        pd.ExtensionCreate("uuid-ossp", false);
                }


                if (column.DotNetType == typeof(PgSqlLTree))
                {
                    if (!pd.ExtensionExists("ltree"))
                        pd.ExtensionCreate("ltree", false);
                }

            }


            try
            {
                var tablebuilder = tableDefinition.GetSqlDefinition(GetConnectionString().TableName, GetConnectionString().SchemaName, throwIfAlreadyExists);
                return Execute().ExecuteScalar(tablebuilder);
            }
            catch (PostgresException ex) when (ex.SqlState == "42P07")
            {
                throw new TableAlreadyExistsException(GetConnectionString().TableName);
            }

        }


        #region Query

        protected IEnumerable<JObject> _Query(params ISelectMember[] clauses)
        {
            var selectStatement = new Select().AddColumnsFromTableDefinition(PostgresTableDefinition);
            if (clauses != null)
                selectStatement.AddClause(clauses);

            selectStatement.From(From.TableOrView(GetTableName()));

            try {
                return Execute().ExecuteReader(selectStatement.GetSqlCommand(PostgresTableDefinition));
            }
            catch (Exception e) {
                Logger.Error(e, $"Query: {selectStatement.GetSqlCommand(PostgresTableDefinition).CommandAsPlainText()}");
                throw;
            }
        }

        protected JObject _QueryByPrimaryKey(object value)
        {
            var prNames = PostgresTableDefinition.PrimaryKeys();
            Dictionary<string, object> dict = new Dictionary<string, object>();
            if (value.IsDictionaryType() || value.CanConvertToDictionary())
            {
                dict = value.ToColumsDictionary();
            }
            else
            {
                if (prNames.Length > 1)
                    throw new Exception("Table has more than one PrimaryKey, please provide a Dictionary with Key/Value");

                var pKeyName = prNames.First().DbName;
                dict.Add(pKeyName, value);
            }

            var where = new List<IWhere>();
            foreach (var kv in dict)
            {
                where.Add(Where.Create().Eq(kv.Key, kv.Value));
            }

            return _Query(Where.MergeQueriesAND(where), Limit.WithNumber(1)).FirstOrDefault();
        }

        protected Task<IEnumerable<JObject>> _QueryAsync(params ISelectMember[] clauses)
        {
            var selectStatement = new Select().AddColumnsFromTableDefinition(PostgresTableDefinition);
            if (clauses != null)
                selectStatement.AddClause(clauses);

            selectStatement.From(From.TableOrView(GetTableName()));

            try
            {
                return Execute().ExecuteReaderAsync(selectStatement.GetSqlCommand(PostgresTableDefinition));
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Query: {selectStatement.GetSqlCommand(PostgresTableDefinition).CommandAsPlainText()}");
                throw;
            }
        }

        protected Task<JObject> _QueryByPrimaryKeyAsync(object value)
        {
            var prNames = PostgresTableDefinition.PrimaryKeys();
            Dictionary<string, object> dict = new Dictionary<string, object>();
            if (value.IsDictionaryType() || value.CanConvertToDictionary())
            {
                dict = value.ToColumsDictionary();
            }
            else
            {
                if (prNames.Length > 1)
                    throw new Exception("Table has more than one PrimaryKey, please provide a Dictionary with Key/Value");

                var pKeyName = prNames.First().DbName;
                dict.Add(pKeyName, value);
            }

            var where = new List<IWhere>();
            foreach (var kv in dict)
            {
                where.Add(Where.Create().Eq(kv.Key, kv.Value));
            }

            return _QueryAsync(Where.MergeQueriesAND(where), Limit.WithNumber(1)).FirstOrDefaultAsync();
        }

        #endregion

        #region Insert

        protected JObject Insert<TAny>(TAny document, List<string> returnValues = null) {
            return Insert<TAny>(new[] {document}, returnValues).FirstOrDefault();
        }
        protected IEnumerable<JObject> Insert<TAny>(IEnumerable<TAny> documents, List<string> returnValues = null) {
            var insert = Statements.Insert.Into(GetTableName())
                .AddColumnsFromTableDefinition(PostgresTableDefinition);

            foreach (var document in documents)
            {
                var dict = document.ToColumsDictionary();
                foreach (var col in PostgresTableDefinition.Columns())
                {
                    if (!col.CanBeNull && !String.IsNullOrWhiteSpace(col.DefaultValue))
                    {
                        var colName = col.ClrName.ToNull() ?? col.DbName.ToNull();

                        if (dict.ContainsKey(colName))
                        {
                            if (dict[colName] == null)
                            {
                                dict.Remove(colName);
                            }
                        }
                    }

                }
                insert.AddValuesFromObject(dict);
            }

            if (returnValues == null || !returnValues.Any())
            {
                returnValues = PostgresTableDefinition.PrimaryKeys().Select(p => p.DbName).ToList();
            }

            if (returnValues.Any())
                insert.AddClause(Returning.Columns(returnValues.ToArray()));

            return Execute().ExecuteReader(insert.GetSqlCommand(PostgresTableDefinition));
        }

        protected Task<JObject> InsertAsync<TAny>(TAny document, List<string> returnValues = null)
        {
            return InsertAsync<TAny>(new[] { document }, returnValues).FirstOrDefaultAsync();
        }
        protected Task<IEnumerable<JObject>> InsertAsync<TAny>(IEnumerable<TAny> documents, List<string> returnValues = null)
        {
            var insert = Statements.Insert.Into(GetTableName())
                .AddColumnsFromTableDefinition(PostgresTableDefinition);
            foreach (var document in documents)
            {
                var dict = document.ToColumsDictionary();
                foreach (var col in PostgresTableDefinition.Columns())
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
                returnValues = PostgresTableDefinition.PrimaryKeys().Select(p => p.DbName).ToList();
            }

            if (returnValues.Any())
                insert.AddClause(Returning.Columns(returnValues.ToArray()));

            return Execute().ExecuteReaderAsync(insert.GetSqlCommand(PostgresTableDefinition));
        }


        #endregion

        #region Update

        public int Update(IWhere query, object document)
        {
            var update = Statements.Update.Table(GetTableName()).SetValueFromObject(document).Where(query);

            Logger.Debug(() => update.GetSqlCommand(PostgresTableDefinition).CommandAsPlainText());

            try {
                return Execute().ExecuteNonQuery(update.GetSqlCommand(PostgresTableDefinition));
            }
            catch (Exception e) {
                Logger.Error(e, "Update", update.GetSqlCommand(PostgresTableDefinition).CommandAsPlainText());
                throw;
            }

        }

        public Task<int> UpdateAsync(IWhere query, object document)
        {
            var update = Statements.Update.Table(GetTableName()).SetValueFromObject(document).Where(query);

            Logger.Debug(() => update.GetSqlCommand(PostgresTableDefinition).CommandAsPlainText());

            try
            {
                return Execute().ExecuteNonQueryAsync(update.GetSqlCommand(PostgresTableDefinition));
            }
            catch (Exception e)
            {
                Logger.Error(e, "Update", update.GetSqlCommand(PostgresTableDefinition).CommandAsPlainText());
                throw;
            }

        }

        #endregion

        #region Delete
        public int Delete(IWhere query)
        {

            var delStatement = Statements.Delete.From(From.TableOrView(GetTableName())).Where(query);
            Logger.Debug(() => delStatement.GetSqlCommand(PostgresTableDefinition).CommandAsPlainText());

            try {
                return Execute().ExecuteNonQuery(delStatement.GetSqlCommand(PostgresTableDefinition));
            }
            catch (Exception e) {
                Logger.Error(e, "Delete", delStatement.GetSqlCommand(PostgresTableDefinition).CommandAsPlainText());
                throw;
            }

        }
        public Task<int> DeleteAsync(IWhere query)
        {

            var delStatement = Statements.Delete.From(From.TableOrView(GetTableName())).Where(query);
            Logger.Debug(() => delStatement.GetSqlCommand(PostgresTableDefinition).CommandAsPlainText());

            try
            {
                return Execute().ExecuteNonQueryAsync(delStatement.GetSqlCommand(PostgresTableDefinition));
            }
            catch (Exception e)
            {
                Logger.Error(e, "Delete", delStatement.GetSqlCommand(PostgresTableDefinition).CommandAsPlainText());
                throw;
            }

        }
        #endregion

        #region Exists
        public bool Exists(IWhere where)
        {
            var from = From.TableOrView(GetTableName());
            var sel = new Select().From(from).Where(where);

            var exists = Select.Exists(new Exists(sel));


            try
            {
                return Execute().ExecuteScalar<bool>(exists.GetSqlCommand(PostgresTableDefinition));
            }
            catch (Exception e)
            {
                Logger.Error(e, "Exists", exists.GetSqlCommand(PostgresTableDefinition).CommandAsPlainText());
                throw;
            }

        }
        public Task<bool> ExistsAsync(IWhere where)
        {

            var sel = new Select().From(From.TableOrView(GetTableName())).Where(where);

            var exists = Select.Exists(new Exists(sel));

            try {
                return Execute().ExecuteScalarAsync<bool>(exists.GetSqlCommand(PostgresTableDefinition));
            }
            catch (Exception e) {
                Logger.Error(e, "Exists", exists.GetSqlCommand(PostgresTableDefinition).CommandAsPlainText());
                throw;
            }

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