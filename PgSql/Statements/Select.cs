using System;
using doob.PgSql.Clauses;
using doob.PgSql.Interfaces;
using doob.PgSql.Interfaces.Where;

namespace doob.PgSql.Statements
{
    public class Select : ISQLCommand
    {
        private object _distinct = null;
        private SelectList _selectList = new SelectList();
        internal From _from;
        private IWhere _where;
        private GroupBy _groupBy;
        private OrderBy _orderBy;
        private Limit _limit;
        private Offset _offset;
        private For _for;
        private Exists _exists;

        public Select AddColumn(SelectListColumn field)
        {
            _selectList.AddColumn(field);
            return this;
        }
        public Select AddColumn(string fieldName)
        {
            var f = new SelectListColumn(fieldName);
            AddColumn(f);
            return this;
        }
        public Select AddColumn(string fieldName, string alias)
        {
            var f = new SelectListColumn(fieldName, alias);
            AddColumn(f);
            return this;
        }
        public Select AddColumn(string table, string fieldName, string alias)
        {
            var f = new SelectListColumn(table, fieldName, alias);
            AddColumn(f);
            return this;
        }

        public Select Distinct()
        {
            _distinct = true;
            return this;
        }
        public Select Distinct(bool distinct)
        {
            _distinct = distinct;
            return this;
        }
        public Select Distinct(string on)
        {
            _distinct = on;
            return this;
        }

        public static ISQLCommand Exists(Exists existsClause)
        {
            var sel = new Select();
            return sel.AddClause(existsClause);
        }

        public Select From(From from)
        {
            return AddClause(from);
        }

        public Select AddColumnsFromTableDefinition(TableDefinition tableDefinition)
        {
            foreach (var pgSqlColumn in tableDefinition.Columns())
            {
                _selectList.AddColumn(pgSqlColumn);
            }
            return this;
        }

        public Select AddColumnsFromType<T>()
        {
            var tbd = TableDefinition.FromType<T>();
            return AddColumnsFromTableDefinition(tbd);
        }

        public Select AddColumnsFromType(Type type)
        {
            var tbd = TableDefinition.FromType(type);
            return AddColumnsFromTableDefinition(tbd);
        }

        public Select Where(IWhere where)
        {
            return AddClause(where);
        }

        public Select GroupBy(GroupBy groupBy)
        {
            return AddClause(groupBy);
        }

        public Select OrderBy(OrderBy orderBy)
        {
            return AddClause(orderBy);
        }

        public Select Limit(Limit limit)
        {
            return AddClause(limit);
        }

        public Select Limit(int limit)
        {
            var lim = Clauses.Limit.WithNumber(limit);
            return AddClause(lim);
        }

        public Select Offset(Offset offset)
        {
            return AddClause(offset);
        }

        public Select Offset(int offset)
        {
            var _offset = Clauses.Offset.WithNumber(offset);
            return AddClause(_offset);
        }

        public Select For(For @for)
        {
            return AddClause(@for);
        }

        public Select AddClause(params ISelectMember[] clause)
        {
            foreach (var selectMember in clause)
            {
                AddClause(selectMember);
            }
            return this;
        }

        public Select AddClause(ISelectMember clause)
        {

            switch (clause)
            {
                case From from:
                    _from = from;
                    break;
                case IWhere where:
                    _where = where;
                    break;
                case GroupBy groupBy:
                    _groupBy = groupBy;
                    break;
                case OrderBy orderBy:
                    _orderBy = orderBy;
                    break;
                case Offset offset:
                    _offset = offset;
                    break;
                case Limit limit:
                    _limit = limit;
                    break;
                case For @for:
                    _for = @for;
                    break;
                case Exists exists:
                    _exists = exists;
                    break;
            }

            return this;
        }


        public PgSqlCommand GetSqlCommand()
        {
            return GetSqlCommand(null);
        }


        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();

            sqlCommand.AppendCommand("SELECT ");

            if (_exists != null)
            {
                var existsCommand = _exists.GetSqlCommand(tableDefinition);
                sqlCommand.AppendCommand(existsCommand.Command, existsCommand.Parameters);
                return sqlCommand;
            }

            if (_distinct != null)
            {
                if (_distinct is bool)
                {
                    sqlCommand.AppendCommand($"DISTINCT ");
                }
                else if (_distinct is string)
                {
                    sqlCommand.AppendCommand($"DISTINCT ON ({_distinct}) ");
                }

            }

            var selectComm = _selectList.GetSqlCommand(tableDefinition);
            sqlCommand.AppendCommandLine($"{selectComm.Command}", selectComm.Parameters);


            if (_from != null)
            {
                var fromComm = _from.GetSqlCommand(tableDefinition);
                sqlCommand.AppendCommandLine($"FROM {fromComm.Command}", fromComm.Parameters);
            }

            if (_where != null)
            {
                var whereComm = _where.GetSqlCommand(tableDefinition);
                if (!String.IsNullOrWhiteSpace(whereComm.Command))
                    sqlCommand.AppendCommandLine($"WHERE {whereComm.Command}", whereComm.Parameters);
            }

            if (_groupBy != null)
            {
                var groupByComm = _groupBy.GetSqlCommand(tableDefinition);
                sqlCommand.AppendCommandLine($"GROUP BY {groupByComm.Command}");
            }

            if (_orderBy != null)
            {
                var orderByComm = _orderBy.GetSqlCommand(tableDefinition);
                sqlCommand.AppendCommandLine($"ORDER BY {orderByComm.Command}");
            }

            if (_offset != null)
            {
                var offsetComm = _offset.GetSqlCommand(tableDefinition);
                sqlCommand.AppendCommandLine($"OFFSET {offsetComm.Command}");
            }


            if (_limit != null)
            {
                var limitComm = _limit.GetSqlCommand(tableDefinition);
                sqlCommand.AppendCommandLine($"LIMIT {limitComm.Command}");
            }

            if (_for != null)
            {
                var forComm = _for.GetSqlCommand(tableDefinition);
                sqlCommand.AppendCommandLine($"FOR {forComm.Command}");
            }

            return sqlCommand;
        }
    }
}
