using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.Helper;
using doob.PgSql.Interfaces;
using doob.PgSql.Interfaces.Where;
using doob.PgSql.Interfaces.Where.Typed;
using doob.PgSql.Statements;

namespace doob.PgSql.Tables
{
    public class TypedTable<T> : BaseTable<TypedTable<T>, T>
    {
        public TypedTable() : base() { }
        public TypedTable(string connectionstring) : base(connectionstring) { }
        public TypedTable(ConnectionString connection) : base(connection) { }
        public TypedTable(ConnectionStringBuilder connectionBuilder) : base(connectionBuilder ){ }


        public new ObjectTable NotTyped()
        {
            return base.NotTyped();
        }

        public new TypedTable<T> ConfigureCertificateEncryption(string certificatePath, string password)
        {
            _secureDataManager = new SecureDataManager(certificatePath, password);
            return this;
        }

        public ITypedWhereClauseLogicalBase<T> Where => Clauses.Where.Create<T>();

        #region Query

        public T[] Query(params ISelectMember[] clauses)
        {
            var selectStatement = new Select().AddColumnsFromType<T>();
            if(clauses != null)
                selectStatement.AddClause(clauses);

            return Query(selectStatement);
        }

        public new T[] Query(Select select) {
            var queryResult = base.Query(select);
            return queryResult.Select(JSON.ToObject<T>).ToArray();
        }

        public new T[] Query(string sqlQuery)
        {
            return base.Query(sqlQuery).Select(JSON.ToObject<T>).ToArray();
        }

        public new T QueryByPrimaryKey(object value)
        {
            return JSON.ToObject<T>(base.QueryByPrimaryKey(value));
        }

        #endregion


        #region Insert

        public Dictionary<string, object> Insert(T document)
        {
            return base.Insert(document);
        }

        public Dictionary<string, object> Insert(T document, List<string> returnValues)
        {
            return base.Insert(document, returnValues);
        }

        public List<Dictionary<string, object>> Insert(IEnumerable<T> documents)
        {
            return base.Insert((IEnumerable<object>)documents);
        }

        public List<Dictionary<string, object>> Insert(IEnumerable<T> documents, List<string> returnValues)
        {
            return base.Insert((IEnumerable<object>)documents, returnValues);
        }

        #endregion


        #region Update

        public object Update(IWhere query, T document)
        {
            return base.Update(query, document);
        }

        public new object Update(IWhere query, object document) {
            return base.Update(query, document);
        }

        #endregion


        #region Remove

        public new int Delete(IWhere query)
        {
            return base.Delete(query);
        }

        #endregion


        #region Exists

        public bool Exists(IWhere query)
        {
            return base.Exists(query, null);
        }

        public bool Exists(Expression<Func<T, object>> field, object value)
        {
            return base.Exists(field.GetPropertyName(), value, null);
        }

        #endregion

    }
}