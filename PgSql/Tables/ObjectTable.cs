using System.Collections.Generic;
using System.Linq;
using doob.PgSql.Helper;
using doob.PgSql.Interfaces;
using doob.PgSql.Interfaces.Where;
using doob.PgSql.Statements;

namespace doob.PgSql.Tables
{
    public class ObjectTable : BaseTable<ObjectTable, object>, ITable
    {

        public ObjectTable() : base() { }
        public ObjectTable(string connectionstring) : base(connectionstring) { }
        public ObjectTable(ConnectionString connection) : base(connection) { }
        public ObjectTable(ConnectionStringBuilder connectionBuilder) : base(connectionBuilder ){ }

        public new TableDefinition TableDefinition => base.TableDefinition;

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

        public Dictionary<string, object>[] Query(params ISelectMember[] clauses)
        {
            var selectStatement = new Select().AddColumnsFromTableDefinition(TableDefinition);
            if (clauses != null)
                selectStatement.AddClause(clauses);

            return Query(selectStatement);
        }

        public new Dictionary<string, object>[] Query(Select select)
        {
            return base.Query(select).Select(JSON.ToObject<Dictionary<string, object>>).ToArray();
        }

        public new Dictionary<string, object>[] Query(string sqlQuery)
        {
            return base.Query(sqlQuery).Select(JSON.ToObject<Dictionary<string, object>>).ToArray();
        }


        public new Dictionary<string, object> QueryByPrimaryKey(object value)
        {
            return JSON.ToDictionary(base.QueryByPrimaryKey(value));
        }


        #endregion


        #region Insert

        public new Dictionary<string, object> Insert(object document)
        {
            return base.Insert(document);
        }

        public new Dictionary<string, object> Insert(object document, List<string> returnValues)
        {
            return base.Insert(document, returnValues);
        }


        public List<Dictionary<string, object>> Insert(IEnumerable<object> documents)
        {
            return base.Insert((IEnumerable<object>)documents);
        }

        public List<Dictionary<string, object>> Insert(IEnumerable<object> documents, List<string> returnValues)
        {
            return base.Insert((IEnumerable<object>)documents, returnValues);
        }


        #endregion


        #region Update

        public new object Update(IWhere query, object document)
        {
            return base.Update(query, document);
        }

        #endregion


        #region Delete

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

        #endregion
    }
}
