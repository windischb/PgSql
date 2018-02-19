using System;
using System.Collections.Generic;
using System.Linq;
using doob.PgSql.Interfaces.Where;

namespace doob.PgSql.Clauses
{
    public class MergeWhereClause : IMergeWhereClause, IMergeWhereClauseConnectionAnd, IMergeWhereClauseConnectionOr
    {
        protected List<IWhere> Queries = new List<IWhere>();

        protected string ChainOperator;

        internal MergeWhereClause()
        {
            
        }

        internal MergeWhereClause(IWhere query)
        {
            Queries.Add(query);
        }

        private void SetChainOperator(string @operator)
        {
            if (String.IsNullOrWhiteSpace(ChainOperator))
                ChainOperator = $" {@operator.Trim()} ";
        }

        public IMergeWhereClauseConnectionAnd AndQuery(IWhere query)
        {
            SetChainOperator("AND");
            Queries.Add(query);
            return this;
        }

        public IMergeWhereClauseConnectionOr OrQuery(IWhere query)
        {
            SetChainOperator("OR");
            Queries.Add(query);
            return this;
        }


        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var command = new PgSqlCommand();
            var lsql = new List<string>();
            foreach (var query in Queries)
            {
                var comm = query.GetSqlCommand(tableDefinition);
                comm.Parameters.ForEach(p =>
                {
                    if (command.Parameters.Any(par => par.UniqueId.Equals(p.UniqueId)))
                    {
                        var ng = Guid.NewGuid().ToString("N");
                        comm.Command = comm.Command.Replace($"@{p.UniqueId}", $"@{ng}");
                        p.UniqueId = ng;
                        command.Parameters.Add(p);
                    }
                    else
                    {
                        command.Parameters.Add(p);
                    }
                });
                lsql.Add($"({comm.Command})");
            }

            command.Command = String.Join(ChainOperator, lsql);
            return command;
        }
    }
}
