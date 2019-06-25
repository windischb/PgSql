using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using doob.PgSql.Clauses;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.Helper;
using doob.PgSql.Interfaces;
using doob.PgSql.Interfaces.Where;
using doob.PgSql.Interfaces.Where.NotTyped;
using doob.PgSql.Statements;
using Reflectensions.ExtensionMethods;

namespace doob.PgSql.Tables
{
    public class ObjectTable : BaseTable<ObjectTable, object>
    {

        public TableDefinition TableDefinition { get; }

        public ObjectTable(ConnectionString connection) : this(connection, new TableDefinition()) { }

        public ObjectTable(ConnectionString connection, TableDefinition createDefinition) : base(connection)
        {
            TableDefinition = createDefinition;

        }

        public void CreateIfNotExists(bool throwIfAlreadyExists = false)
        {
            if (!GetSchema().CreateIfNotExists().TableExists(GetConnectionString().TableName))
                _createTable(TableDefinition, throwIfAlreadyExists);
        }

        public void UpdateTableDefinition()
        {
            throw new NotImplementedException();
        }

        public new TypedTable<T> Typed<T>()
        {
            return base.Typed<T>();
        }

        public new ObjectTable ConfigureCertificateEncryption(string certificatePath, string password)
        {
            SecureDataManager = new SecureDataManager(certificatePath, password);
            return this;
        }

        #region Query

        public IEnumerable<Dictionary<string, object>> Query(Func<IWhereClauseLogicalBase, IWhere> where)
        {
            var q = where.Invoke(Where.Create());
            return Query(q);
        }

        public IEnumerable<Dictionary<string, object>> Query(params ISelectMember[] clauses)
        {
            return base._Query(clauses).Select(item => Converter.Json.ToDictionary(item));
        }

        public Dictionary<string, object> QueryByPrimaryKey(object value)
        {
            return base._QueryByPrimaryKey(value)?.ToObject<Dictionary<string, object>>();
        }


        public Task<IEnumerable<Dictionary<string, object>>> QueryAsync(Func<IWhereClauseLogicalBase, IWhere> where)
        {
            var q = where.Invoke(Where.Create());
            return QueryAsync(q);
        }

        public Task<IEnumerable<Dictionary<string, object>>> QueryAsync(params ISelectMember[] clauses)
        {
            return base._QueryAsync(clauses)?.SelectAsync(item => Converter.Json.ToDictionary(item));
        }

        public Task<Dictionary<string, object>> QueryByPrimaryKeyAsync(object value)
        {
            return base._QueryByPrimaryKeyAsync(value)?.ToObjectAsync<Dictionary<string, object>>();
        }


        #endregion



        #region Insert

        public Dictionary<string, object> Insert(object document, List<string> returnValues = null)
        {
            return Converter.Json.ToObject<Dictionary<string, object>>(base.Insert(document, returnValues));
        }
        public Dictionary<string, object> InsertOrUpdate(object document, List<string> returnValues = null)
        {
            return Converter.Json.ToObject<Dictionary<string, object>>(base.InsertOrUpdate(document, returnValues));
        }

        public IEnumerable<Dictionary<string, object>> Insert(IEnumerable<object> documents, List<string> returnValues = null)
        {
            return base.Insert(documents, returnValues).Select(Converter.Json.ToObject<Dictionary<string, object>>);
        }
        public IEnumerable<Dictionary<string, object>> InsertOrUpdate(IEnumerable<object> documents, List<string> returnValues = null)
        {
            return base.InsertOrUpdate(documents, returnValues).Select(Converter.Json.ToObject<Dictionary<string, object>>);
        }

        public async Task<Dictionary<string, object>> InsertAsync(object document, List<string> returnValues = null)
        {
            return Converter.Json.ToObject<Dictionary<string, object>>(await base.InsertAsync(document, returnValues));
        }
        public async Task<Dictionary<string, object>> InsertOrUpdateAsync(object document, List<string> returnValues = null)
        {
            return Converter.Json.ToObject<Dictionary<string, object>>(await base.InsertOrUpdateAsync(document, returnValues));
        }

        public Task<IEnumerable<Dictionary<string, object>>> InsertAsync(IEnumerable<object> documents, List<string> returnValues = null)
        {
            return base.InsertAsync(documents, returnValues).SelectAsync(Converter.Json.ToObject<Dictionary<string, object>>);
        }
        public Task<IEnumerable<Dictionary<string, object>>> InsertOrUpdateAsync(IEnumerable<object> documents, List<string> returnValues = null)
        {
            return base.InsertOrUpdateAsync(documents, returnValues).SelectAsync(Converter.Json.ToObject<Dictionary<string, object>>);
        }

        #endregion


        #region Update

        protected int Update(Func<IWhereClauseLogicalBase, IWhere> where, object document)
        {
            var q = where.Invoke(Where.Create());
            return Update(q, document);
        }

        protected Task<int> UpdateAsync(Func<IWhereClauseLogicalBase, IWhere> where, object document)
        {
            var q = where.Invoke(Where.Create());
            return UpdateAsync(q, document);
        }

        #endregion


        #region Delete

        protected int Delete(Func<IWhereClauseLogicalBase, IWhere> where)
        {
            var q = where.Invoke(Where.Create());
            return Delete(q);
        }

        protected Task<int> DeleteAsync(Func<IWhereClauseLogicalBase, IWhere> where)
        {
            var q = where.Invoke(Where.Create());
            return DeleteAsync(q);
        }

        #endregion


        #region Exists

        public virtual bool Exists(Func<IWhereClauseLogicalBase, IWhere> where)
        {
            var q = where.Invoke(Where.Create());
            return Exists(q);
        }

        public virtual Task<bool> ExistsAsync(Func<IWhereClauseLogicalBase, IWhere> where)
        {
            var q = where.Invoke(Where.Create());
            return ExistsAsync(q);
        }

        #endregion
    }
}
