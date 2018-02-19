using System;
using System.Collections.Generic;
using System.Linq;
using doob.PgSql.Interfaces;

namespace doob.PgSql.Clauses
{
    public class GroupBy : ISelectMember
    {
        private readonly List<IGroupByClauseItem> _groupByItemList = new List<IGroupByClauseItem>();

        public GroupBy() { }
        public GroupBy(IGroupByClauseItem orderByClauseItem)
        {
            _groupByItemList.Add(orderByClauseItem);
        }
        public GroupBy(params IGroupByClauseItem[] orderByClauseItems)
        {
            _groupByItemList = orderByClauseItems.ToList();
        }

        public GroupBy(IEnumerable<IGroupByClauseItem> orderByClauseItems)
        {
            _groupByItemList = orderByClauseItems.ToList();
        }


        public static GroupBy Name(string name)
        {
            var gb = new GroupBy();
            gb._groupByItemList.Add(new GroupByName(name));
            return gb;
        }

        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();

            if (_groupByItemList.Any())
                sqlCommand.AppendCommand($"{String.Join(", ", _groupByItemList)}");

            return sqlCommand;
        }
    }
}
