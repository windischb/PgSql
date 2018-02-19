using System;
using System.Collections.Generic;
using System.Linq;
using doob.PgSql.Clauses;
using doob.PgSql.Interfaces.Where;
using doob.PgSql.Statements;

namespace doob.PgSql.Tables
{
    public class HistoryTable<T, TItem> where T : BaseTable<T, TItem>
    {
        private readonly BaseTable<T, TItem> _parent;
        private readonly ObjectTable _table;

        public HistoryTable(BaseTable<T, TItem> parentTable)
        {
            _parent = parentTable;
            var td = _parent.GetTableDefinition().Clone();
            td.ClearPrimaryKeys().ClearDefaultValues().ClearUnique();
            td
                .AddColumn(
                    Column.Build<Guid>("#id")
                        .DefaultValue("uuid_generate_v1mc()")
                        .AsPrimaryKey()
                        .SetPosition(0))
                .AddColumn(Column.Build<string>("#action").SetPosition(1))
                .AddColumn(Column.Build<DateTime>("#timestamp").DefaultValue("current_timestamp").SetPosition(2));
                   
            _table = _parent.GetSchema().CreateTable($"{_parent.ConnectionString.TableName}#history", td, false);
            
        }

        public Dictionary<string, object> QueryByPrimaryKey(Dictionary<string, object> id)
        {
            var where = new List<IWhere>();
            foreach (var o in id)
            {
                where.Add(Where.Create().Eq(o.Key, o.Value));
            }

            var selectStatement = new Select().AddColumnsFromTableDefinition(_table.GetTableDefinition()).Where(Where.MergeQueriesAND(where)).Limit(1);
            return _table.Query(selectStatement).FirstOrDefault();
        }

        public HistoryTable<T, TItem> EnsureExists()
        {
            _table.EnsureExists();
            return this;
        }
    }
}
