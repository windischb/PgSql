using System;
using System.Collections.Generic;
using System.Linq;
using doob.PgSql.Attributes;
using doob.PgSql.Clauses;
using doob.PgSql.Exceptions;
using doob.PgSql.Interfaces.Where;
using doob.PgSql.Listener;
using doob.PgSql.Statements;
using Newtonsoft.Json.Linq;

namespace doob.PgSql.Tables
{

    public class HistoryTable : BaseTable<HistoryTable, HistoryEntity<object>>
    {
        private readonly ITable _parentTable;
        private ITable _histTable;

       
       

        public HistoryTable(ITable parentTable) : base(ConnectionString.Build(parentTable.GetConnectionString()).WithSchema($"{parentTable.GetConnectionString().SchemaName}#history"))
        {
            _parentTable = parentTable;
        }

        public HistoryTable CreateIfNotExists()
        {
            var tbl = GetSchema().CreateIfNotExists().CreateTable<HistoryEntity<object>>(GetConnectionString().TableName);
            return EnsureExists();
        }

        public override HistoryTable EnsureExists()
        {
            
            var ret = base.EnsureExists();
            if (!_parentTable.GetDatabase().ExtensionExists("hstore"))
                _parentTable.GetDatabase().ExtensionCreate("hstore", false);

            Execute().ExecuteNonQuery(TableStatements.AddHistoryTrigger(_parentTable.GetSchema()));
            _parentTable.TriggerCreate("doob_pgsql_writeHistory", "WriteHistory", true);
            return ret;
        }
    }

    public class HistoryTable<T> : TypedTable<HistoryEntity<T>>
    {
        private readonly ITable _parentTable;

        public HistoryTable(ITable parentTable) : base(ConnectionString.Build(parentTable.GetConnectionString()).WithSchema($"{parentTable.GetConnectionString().SchemaName}#history"))
        {
            _parentTable = parentTable;
        }

        public HistoryTable<T> CreateIfNotExists()
        {
            var tbl = GetSchema().CreateIfNotExists().CreateTable<HistoryEntity<object>>(GetConnectionString().TableName);
            return EnsureExists();
        }

        public new HistoryTable<T> EnsureExists()
        {

            var ret = (HistoryTable<T>)base.EnsureExists();

            Execute().ExecuteNonQuery(TableStatements.AddHistoryTrigger(_parentTable.GetSchema()));
            _parentTable.TriggerCreate("doob_pgsql_writeHistory", "WriteHistory", true);
            return ret;
        }
    }



    public class HistoryEntity<T>
    {
        [PgSqlPrimaryKey(DefaultValues.Serial.BigSerial)]
        public long Id { get; set; }

        public TriggerAction Action { get; set; }

        public T Old { get; set; }

        public T New { get; set; }

        public Dictionary<string, object> ChangesOld { get; set; }
        public Dictionary<string, object> ChangesNew { get; set; }

        public List<string> ChangedKeys { get; set; }

        [PgSqlDefaultValue(DefaultValues.DateTime.Now)]
        public DateTime Timestamp { get; set; }

    }

    //public class HistoryTable<T> where T : BaseTable<T>
    //{
    //    private readonly BaseTable<T> _parent;
    //    private readonly ObjectTable _table;

    //    public HistoryTable(BaseTable<T> parentTable)
    //    {
    //        _parent = parentTable;
    //        var td = _parent.TableDefinition.Clone();
    //        td.ClearPrimaryKeys().ClearDefaultValues().ClearUnique();
    //        td
    //            .AddColumn(
    //                ColumnBuilder.Build<Guid>("#id")
    //                    .DefaultValue("uuid_generate_v1mc()")
    //                    .AsPrimaryKey()
    //                    .SetPosition(0))
    //            .AddColumn(ColumnBuilder.Build<string>("#action").SetPosition(1))
    //            .AddColumn(ColumnBuilder.Build<DateTime>("#timestamp").DefaultValue("current_timestamp").SetPosition(2));
                   
    //        _table = _parent.GetSchema().CreateTable($"{_parent.GetConnectionString().TableName}#history", td, false);
            
    //    }

    //    public Dictionary<string, object> QueryByPrimaryKey(Dictionary<string, object> id)
    //    {
    //        var where = new List<IWhere>();
    //        foreach (var o in id)
    //        {
    //            where.Add(Where.Create().Eq(o.Key, o.Value));
    //        }

    //        var selectStatement = new Select().AddColumnsFromTableDefinition(_table.TableDefinition).Where(Where.MergeQueriesAND(where)).Limit(1);
    //        return _table.Query(selectStatement).FirstOrDefault();
    //    }

    //    public HistoryTable<T> EnsureExists()
    //    {
    //        _table.EnsureExists();
    //        return this;
    //    }
    //}

    //public class HistoryEntry
    //{
    //    public Guid Id { get; set; }
    //}
}
