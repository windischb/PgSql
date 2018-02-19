using doob.PgSql.ExtensionMethods;
using doob.PgSql.Interfaces;

namespace doob.PgSql.Clauses
{
    public class OrderByClauseName : IOrderByClauseItem
    {
        public string Name { get; set; }
        public bool SortDescending { get; set; }
        public bool InvertNullOrder { get; set; }

        private OrderByClauseName() { }

        public OrderByClauseName(string name)
        {
            Name = name.ClearString();
        }
        public OrderByClauseName(string name, bool sortDescending)
        {
            Name = name.ClearString();
            SortDescending = sortDescending;
        }
        public OrderByClauseName(string name, bool sortDescending, bool invertNullOrder)
        {
            Name = name.ClearString();
            SortDescending = sortDescending;
            InvertNullOrder = invertNullOrder;
        }

        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();

            sqlCommand.AppendCommand($"\"{Name}\"");
            if (SortDescending)
            {
                sqlCommand.AppendCommand($" DESC");
            }
            if (InvertNullOrder)
            {
                sqlCommand.AppendCommand($" {(SortDescending ? "NULLS LAST" : "NULLS FIRST")}");
            }

            return sqlCommand;
        }
    }
}
