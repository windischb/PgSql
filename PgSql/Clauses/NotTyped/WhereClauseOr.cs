using System;
using System.Collections.Generic;
using doob.PgSql.Expressions;
using doob.PgSql.Expressions.Logical;
using doob.PgSql.Interfaces.Where.NotTyped;

namespace doob.PgSql.Clauses.NotTyped
{
    public class WhereClauseOr : IWhereClauseLogicalOr, IWhereClauseConnectionOr
    {
        protected List<ExpressionBase> Xpressions = new List<ExpressionBase>();

        internal WhereClauseOr(ExpressionBase firstxpressions)
        {
            Xpressions.Add(firstxpressions);
        }

        public IWhereClauseLogicalOr Not
        {
            get
            {
                var expr = new ExpressionNot();
                Xpressions.Add(expr);
                return this;
            }
            
        }

        public IWhereClauseConnectionOr Eq(string propertyName, object value)
        {
            var expr = new ExpressionEqual(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public IWhereClauseConnectionOr Lt(string propertyName, object value)
        {
            var expr = new ExpressionLowerThan(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public IWhereClauseConnectionOr Gt(string propertyName, object value)
        {
            var expr = new ExpressionGreaterThan(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public IWhereClauseConnectionOr Lte(string propertyName, object value)
        {
            var expr = new ExpressionLowerThanOrEqual(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public IWhereClauseConnectionOr Gte(string propertyName, object value)
        {
            var expr = new ExpressionGreaterThanOrEqual(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public IWhereClauseConnectionOr Between(string propertyName, object min, object max)
        {
            var expr = new ExpressionBetween(propertyName, min, max);
            Xpressions.Add(expr);
            return this;
        }

        public IWhereClauseConnectionOr IsNull(string propertyName)
        {
            var expr = new ExpressionIsNull(propertyName);
            Xpressions.Add(expr);
            return this;
        }

        public IWhereClauseConnectionOr IsNotNull(string propertyName)
        {
            var expr = new ExpressionIsNotNull(propertyName);
            Xpressions.Add(expr);
            return this;
        }

        public IWhereClauseConnectionOr Any(string propertyName, params object[] value)
        {
            var expr = new ExpressionAny(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public IWhereClauseConnectionOr Like(string propertyName, string value)
        {
            var expr = new ExpressionLike(propertyName, value, true);
            Xpressions.Add(expr);
            return this;
        }

        public IWhereClauseConnectionOr Like(string propertyName, string value, bool ignoreCase)
        {
            var expr = new ExpressionLike(propertyName, value, ignoreCase);
            Xpressions.Add(expr);
            return this;
        }

        public IWhereClauseConnectionOr LTreeMatch(string propertyName, string value)
        {
            var expr = new ExpressionLTreeMatch(propertyName, value);
            Xpressions.Add(expr);
            return this;
        }

        public IWhereClauseLogicalOr Or => this;


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
                if (xpression.ColumnName != null)
                {
                    column = tableDefinition.GetColumnBuilderByDbName(xpression.ColumnName);
                }
                var comm = xpression.GetSqlCommand(column);
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
            command.Command = String.Join(" OR ", lsql);
            return command;
        }
    }
}