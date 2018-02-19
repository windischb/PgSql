using System.Collections.Generic;
using System.Linq;
using doob.PgSql.Clauses.NotTyped;
using doob.PgSql.Expressions;
using doob.PgSql.Expressions.Logical;
using doob.PgSql.Interfaces.Where;
using doob.PgSql.Interfaces.Where.NotTyped;
using doob.PgSql.Interfaces.Where.Typed;

namespace doob.PgSql.Clauses
{
    public class Where : IWhereClauseLogicalBase, IWhereClauseConnectionBase
    {
        
        private ExpressionBase _first;


        private Where() { }


        public IWhereClauseLogicalAnd And
        {
            get
            {
                var andquery = new WhereClauseAnd(_first);
                return andquery;
            }
            
        }

        public IWhereClauseLogicalOr Or
        {
            get
            {
                var orquery = new WhereClauseOr(_first);
                return orquery;
            }
            
        }



        public IMergeWhereClause Merge(IWhere query)
        {
            return new MergeWhereClause(query);
        }

        public IWhereClauseLogicalBase Not
        {
            get
            {
                _first = new ExpressionNot();
                return this;
            }
        }


        public IWhereClauseConnectionBase Eq(string propertyName, object value)
        {
            _first = new ExpressionEqual(propertyName, value);
            return this;
        }

        public IWhereClauseConnectionBase Lt(string propertyName, object value)
        {
            _first = new ExpressionLowerThan(propertyName, value);
            return this;
        }

        public IWhereClauseConnectionBase Gt(string propertyName, object value)
        {
            _first = new ExpressionGreaterThan(propertyName, value);
            return this;
        }

        public IWhereClauseConnectionBase Lte(string propertyName, object value)
        {
            _first = new ExpressionLowerThanOrEqual(propertyName, value);
            return this;
        }

        public IWhereClauseConnectionBase Gte(string propertyName, object value)
        {
            _first = new ExpressionGreaterThanOrEqual(propertyName, value);
            return this;
        }

        public IWhereClauseConnectionBase Between(string propertyName, object min, object max)
        {
            _first = new ExpressionBetween(propertyName, min, max);
            return this;
        }

        public IWhereClauseConnectionBase IsNull(string propertyName)
        {
            _first = new ExpressionIsNull(propertyName);
            return this;
        }

        public IWhereClauseConnectionBase IsNotNull(string propertyName)
        {
            _first = new ExpressionIsNotNull(propertyName);
            return this;
        }

        public IWhereClauseConnectionBase Any(string propertyName, params object[] value)
        {
            _first = new ExpressionAny(propertyName, value);
            return this;
        }

        public IWhereClauseConnectionBase Like(string propertyName, string value)
        {
            _first = new ExpressionLike(propertyName, value, true);
            return this;
        }

        public IWhereClauseConnectionBase Like(string propertyName, string value, bool ignoreCase)
        {
            _first = new ExpressionLike(propertyName, value, ignoreCase);
            return this;
        }

        public IWhereClauseConnectionBase LTreeMatch(string propertyName, string value)
        {
            _first = new ExpressionLTreeMatch(propertyName, value);
            return this;
        }


        public static IWhereClauseLogicalBase Create() => new Where();
        public static ITypedWhereClauseLogicalBase<T> Create<T>() => new TypedWhereClause<T>();

     
        public static IMergeWhereClauseConnectionAnd MergeQueriesAND(IEnumerable<IWhere> queries)
        {
            IMergeWhereClauseConnectionAnd query = new MergeWhereClause();
            foreach (var q in queries)
            {
                query = query.AndQuery(q);
            }
            return query;
        }
        public static IMergeWhereClauseConnectionAnd MergeQueriesAND(params IWhere[] queries)
        {
            return MergeQueriesAND(queries.ToList());
        }

        public static IMergeWhereClauseConnectionOr MergeQueriesOR(IEnumerable<IWhere> queries)
        {
            IMergeWhereClauseConnectionOr query = new MergeWhereClause();
            foreach (var q in queries)
            {
                query = query.OrQuery(q);
            }
            return query;
        }
        public static IMergeWhereClauseConnectionOr MergeQueriesOR(params IWhere[] queries)
        {
            return MergeQueriesOR(queries.ToList());
        }


        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var column = tableDefinition?.GetColumn(_first.ColumnName);
            return _first.GetSqlCommand(column);
        }
    }
}
