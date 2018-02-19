using System;
using System.Collections.Generic;
using System.Linq;
using doob.PgSql.Interfaces;

namespace doob.PgSql.Clauses
{
    public class OrderBy : ISelectMember
    {

        private readonly List<IOrderByClauseItem> _orderByClauseItems = new List<IOrderByClauseItem>();

        private OrderBy() { }
        private OrderBy(IOrderByClauseItem orderByClauseItem)
        {
            _orderByClauseItems.Add(orderByClauseItem);
        }
        private OrderBy(params IOrderByClauseItem[] orderByClauseItems)
        {
            _orderByClauseItems = orderByClauseItems.ToList();
        }

        public static OrderBy Name(string name)
        {
            return new OrderBy().AddName(name);
        }
        public static OrderBy Name(string name, bool sortDescending)
        {
            return new OrderBy().AddName(name, sortDescending);
        }
        public static OrderBy Name(string name, bool sortDescending, bool invertNullOrder)
        {
            return new OrderBy().AddName(name, sortDescending, invertNullOrder);
        }



        public OrderBy AddName(string name)
        {
            var n = new OrderByClauseName(name);
            _orderByClauseItems.Add(n);
            return this;
        }
        public OrderBy AddName(string name, bool sortDescending)
        {
            var n = new OrderByClauseName(name, sortDescending);
            _orderByClauseItems.Add(n);
            return this;
        }
        public OrderBy AddName(string name, bool sortDescending, bool invertNullOrder)
        {
            var n = new OrderByClauseName(name, sortDescending, invertNullOrder);
            _orderByClauseItems.Add(n);
            return this;
        }

        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();

            if (_orderByClauseItems.Any())
                sqlCommand.AppendCommand($"{String.Join(",", _orderByClauseItems.Select(i => i.GetSqlCommand(tableDefinition).Command))}");

            return sqlCommand;
        }
    }
}
