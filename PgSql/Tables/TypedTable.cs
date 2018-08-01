using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.Helper;
using doob.PgSql.Interfaces;
using doob.PgSql.Interfaces.Where;
using doob.PgSql.Interfaces.Where.Typed;
using doob.PgSql.Listener;
using doob.PgSql.Statements;

namespace doob.PgSql.Tables
{
    public class TypedTable<T> : BaseTable<TypedTable<T>, T>, ITable<T>
    {

        public TableDefinition<T> TableDefinition { get; }

        protected internal TypedTable(ConnectionString connection) : this(connection, Build.TableDefinition<T>()) {
            
        }

        protected internal TypedTable(ConnectionString connection, TableDefinition<T> createDefinition) : base(connection)
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



        public new ObjectTable NotTyped()
        {
            return base.NotTyped();
        }

        public new TypedTable<T> ConfigureCertificateEncryption(string certificatePath, string password)
        {
            SecureDataManager = new SecureDataManager(certificatePath, password);
            return this;
        }

        public ITypedWhereClauseLogicalBase<T> Where => Clauses.Where.Create<T>();


        internal IQueryable<T> Queryable()
        {
            return PgSqlQueryFactory.Queryable<T>(this);
        }

       
        private TypedTableListener<T> _onNotification;
        public new TypedTableListener<T> Notifications(NotificationSharedBy sharedBy = NotificationSharedBy.Schema) => _onNotification ?? (_onNotification = DMLTriggerManager.GetTypedTiggerListener<T>(this, sharedBy));




        #region Query

        public IEnumerable<T> Query(Func<ITypedWhereClauseLogicalBase<T>, IWhere> where)
        {
            var q = where.Invoke(Clauses.Where.Create<T>());
            return Query(q);
        }

        public IEnumerable<T> Query(params ISelectMember[] clauses)
        {
            return base._Query(clauses).Select(JSON.ToObject<T>);
        }

        public T QueryByPrimaryKey(object value)
        {
            return base._QueryByPrimaryKey(value).ToObject<T>();
        }



        public Task<IEnumerable<T>> QueryAsync(Func<ITypedWhereClauseLogicalBase<T>, IWhere> where)
        {
            var q = where.Invoke(Clauses.Where.Create<T>());
            return QueryAsync(q);
        }

        public Task<IEnumerable<T>> QueryAsync(params ISelectMember[] clauses)
        {
            return base._QueryAsync(clauses)?.SelectAsync(JSON.ToObject<T>);
        }

        public Task<T> QueryByPrimaryKeyAsync(object value)
        {
            return base._QueryByPrimaryKeyAsync(value)?.ToObjectAsync<T>();
        }

        #endregion


        #region Insert

        public Dictionary<string, object> Insert(T document, List<string> returnValues = null)
        {
            return base.Insert(document, returnValues)?.ToObject<Dictionary<string, object>>();
        }
        public IEnumerable<Dictionary<string, object>> Insert(IEnumerable<T> documents, List<string> returnValues = null)
        {
            return base.Insert((IEnumerable<object>)documents, returnValues).Select(JSON.ToObject<Dictionary<string, object>>);
        }

        public Task<Dictionary<string, object>> InsertAsync(T document, List<string> returnValues = null)
        {
            return base.InsertAsync(document, returnValues)?.ToObjectAsync<Dictionary<string, object>>();
        }
        public Task<IEnumerable<Dictionary<string, object>>> InsertAsync(IEnumerable<T> documents, List<string> returnValues = null)
        {
            return base.InsertAsync((IEnumerable<object>)documents, returnValues).SelectAsync(JSON.ToObject<Dictionary<string, object>>);
        }

        #endregion


        #region Update

        public int Update(Func<ITypedWhereClauseLogicalBase<T>, IWhere> where, T document)
        {
            var q = where.Invoke(Clauses.Where.Create<T>());

            return base.Update(q, document);
        }

        public int Update(Func<ITypedWhereClauseLogicalBase<T>, IWhere> where, object document) {

            var q = where.Invoke(Clauses.Where.Create<T>());

            return base.Update(q, document);
        }

        public Task<int> UpdateAsync(Func<ITypedWhereClauseLogicalBase<T>, IWhere> where, T document)
        {
            var q = where.Invoke(Clauses.Where.Create<T>());

            return base.UpdateAsync(q, document);
        }

        public Task<int> UpdateAsync(Func<ITypedWhereClauseLogicalBase<T>, IWhere> where, object document)
        {

            var q = where.Invoke(Clauses.Where.Create<T>());

            return base.UpdateAsync(q, document);
        }

        #endregion


        #region Remove

        public int Delete(Func<ITypedWhereClauseLogicalBase<T>, IWhere> where)
        {
            var q = where.Invoke(Clauses.Where.Create<T>());
            return base.Delete(q);
        }

        public Task<int> DeleteAsync(Func<ITypedWhereClauseLogicalBase<T>, IWhere> where)
        {
            var q = where.Invoke(Clauses.Where.Create<T>());
            return base.DeleteAsync(q);
        }

        #endregion


        #region Exists

        public bool Exists(Func<ITypedWhereClauseLogicalBase<T>, IWhere> where)
        {
            var q = where.Invoke(Clauses.Where.Create<T>());
            return Exists(q);
        }

        public Task<bool> ExistsAsync(Func<ITypedWhereClauseLogicalBase<T>, IWhere> where)
        {
            var q = where.Invoke(Clauses.Where.Create<T>());
            return ExistsAsync(q);
        }

        #endregion



        public new HistoryTable<T> History()
        {
            return new HistoryTable<T>(this);
        }

    }
}