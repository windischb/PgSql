using System;
using System.Collections.Generic;
using doob.PgSql.Expressions;
using doob.PgSql.Expressions.Logical;
using doob.PgSql.Interfaces.Where.NotTyped;

namespace doob.PgSql.Clauses.NotTyped
{
    public class WhereClauseAnd : IWhereClauseLogicalAnd, IWhereClauseConnectionAnd
    {

        protected List<ExpressionBase> Xpressions = new List<ExpressionBase>();

        internal WhereClauseAnd(ExpressionBase firstxpressions)
        {
            Xpressions.Add(firstxpressions);
        }

        public IWhereClauseLogicalAnd Not
        {
            get
            {
                var expr = new ExpressionNot();
                Xpressions.Add(expr);
                return this;
            }
            
        }

        public IWhereClauseConnectionAnd Eq(string propertyName, object value)
        {
            var expr = new ExpressionEqual(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public IWhereClauseConnectionAnd Lt(string propertyName, object value)
        {
            var expr = new ExpressionLowerThan(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public IWhereClauseConnectionAnd Gt(string propertyName, object value)
        {
            var expr = new ExpressionGreaterThan(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public IWhereClauseConnectionAnd Lte(string propertyName, object value)
        {
            var expr = new ExpressionLowerThanOrEqual(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public IWhereClauseConnectionAnd Gte(string propertyName, object value)
        {
            var expr = new ExpressionGreaterThanOrEqual(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public IWhereClauseConnectionAnd Between(string propertyName, object min, object max)
        {
            var expr = new ExpressionBetween(propertyName, min, max);
            Xpressions.Add(expr);
            return this;
        }

        public IWhereClauseConnectionAnd IsNull(string propertyName)
        {
            var expr = new ExpressionIsNull(propertyName);
            Xpressions.Add(expr);
            return this;
        }

        public IWhereClauseConnectionAnd IsNotNull(string propertyName)
        {
            var expr = new ExpressionIsNotNull(propertyName);
            Xpressions.Add(expr);
            return this;
        }

        public IWhereClauseConnectionAnd Any(string propertyName, params object[] value)
        {
            var expr = new ExpressionAny(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public IWhereClauseConnectionAnd Like(string propertyName, string value)
        {
            var expr = new ExpressionLike(propertyName, value, true);
            Xpressions.Add(expr);
            return this;
        }

        public IWhereClauseConnectionAnd Like(string propertyName, string value, bool ignoreCase)
        {
            var expr = new ExpressionLike(propertyName, value, ignoreCase);
            Xpressions.Add(expr);
            return this;
        }

        public IWhereClauseConnectionAnd LTreeMatch(string propertyName, string value)
        {
            var expr = new ExpressionLTreeMatch(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public IWhereClauseLogicalAnd And 
        {
            get
            {
                return this;
            }
        }



        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            
            var command = new PgSqlCommand();
            var lsql = new List<string>();
            bool not = false;

            foreach (var xpression in Xpressions)
            {
                if (xpression is ExpressionNot)
                {
                    not = true;
                    continue;
                }


                ColumnBuilder column = null;
                if(tableDefinition != null)
                    if (xpression.ColumnName != null)
                    {
                        column = tableDefinition.GetColumn(xpression.ColumnName);
                    }

                PgSqlCommand comm = xpression.GetSqlCommand(column);
                if (not)
                    comm.Command = $"NOT {comm.Command}";

                comm.Parameters.ForEach(p =>
                {
                    if (command.ParameterIdExists(p.UniqueId))
                    {
                        var newParam = p.RebuildWithNewId();
                        comm.Command = comm.Command.Replace($"@{p.UniqueId}", $"@{newParam.UniqueId}");
                        command.Parameters.Add(newParam);
                    }
                    else
                    {
                        command.Parameters.Add(p);
                    }
                });
                lsql.Add($"({comm.Command})");

                not = false;
            }
            command.Command = String.Join(" AND ", lsql);
            return command;
        }

        
    }
}