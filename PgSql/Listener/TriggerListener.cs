using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.Helper;
using doob.PgSql.Tables;
using Npgsql;

namespace doob.PgSql.Listener
{
    public class TriggerListener : BaseListener
    {

        public IObservable<TriggerNotification> Notifications() => _tableNotificationSubject.AsObservable();


        public TriggerListener(ConnectionStringBuilder connectionBuilder) : base(connectionBuilder)
        {

        }

        public TriggerListener ListenOnTablesFqdn(params string[] fqdnTableNames)
        {
            foreach (var tableName in fqdnTableNames)
            {
                if (!tableName.Contains('.'))
                    throw new Exception($"TableName '{tableName}' is not a Full Qualified Name!");

                _listenOnTables.Add(tableName);
            }

            return this;
        }

        public TriggerListener ListenOnTables(params string[] tableNames)
        {
            foreach (var tableName in tableNames)
            {
                string _schema = "*";
                if (!String.IsNullOrWhiteSpace(_connectionBuilder.GetConnection().SchemaName))
                {
                    _schema = _connectionBuilder.GetConnection().SchemaName;
                }
                _listenOnTables.Add($"{_schema}.{tableName}");
            }

            return this;
        }

        private ListenMode _listenMode;

        public void StartListening(ListenMode listenMode)
        {
            _listenMode = listenMode;
            if (!_listenOnTables.Any())
            {
                string _schema = "*";
                if (!String.IsNullOrWhiteSpace(_connectionBuilder.GetConnection().SchemaName))
                {
                    _schema = _connectionBuilder.GetConnection().SchemaName;
                }
                string _table = "*";
                if (!String.IsNullOrWhiteSpace(_connectionBuilder.GetConnection().TableName))
                {
                    _table = _connectionBuilder.GetConnection().TableName;
                }

                _listenOnTables.Add($"{_schema}.{_table}");
            }

            var notifyName = "";
            switch (listenMode)
            {
                case ListenMode.HistoryTableId:
                case ListenMode.HistoryTableEntry:
                    notifyName = $"XA-Notify-TableEvent_ByHistory";
                    break;
                case ListenMode.ReferenceTablePrimaryKeys:
                case ListenMode.ReferenceTableEntry:
                    notifyName = $"XA-Notify-TableEvent_ByReference";
                    break;
                default:
                    throw new NotImplementedException(listenMode.ToString());
            }
            _startListening(notifyName);
        }

        public void StopListening()
        {
            _stopListening();
        }



        protected readonly Subject<TriggerNotification> _tableNotificationSubject = new Subject<TriggerNotification>();


        protected readonly List<string> _listenOnTables = new List<string>();


        protected override void _OnEvent(object sender, NpgsqlNotificationEventArgs npgsqlNotificationEventArgs)
        {

            var notifyObject = JSON.ToObject<NotificationObject>(npgsqlNotificationEventArgs.AdditionalInformation);

            var fqdn = $"{notifyObject.Schema}.{notifyObject.Table}";

            var notify = false;
            foreach (var s in _listenOnTables)
            {
                if (Wildcard.IsMatch(fqdn, s))
                {
                    notify = true;
                    break;
                }

            }

            if (!notify)
                return;


            Dictionary<string, object> returnObject = null;

            ObjectTable table;
            switch (_listenMode)
            {
                case ListenMode.ReferenceTablePrimaryKeys:
                    returnObject = notifyObject.Data;
                    break;
                case ListenMode.ReferenceTableEntry:
                    if (notifyObject.Action == TriggerAction.Delete)
                    {
                        returnObject = notifyObject.Data;
                    } else
                    {
                        table = GetTable(notifyObject.Table);

                        returnObject = table.QueryByPrimaryKey(notifyObject.Data).CloneTo<Dictionary<string, object>>();
                    }
                    break;
                case ListenMode.HistoryTableId:
                    returnObject = notifyObject.Data;
                    break;
                case ListenMode.HistoryTableEntry:
                    table = GetTable(notifyObject.Table);
                    returnObject = table.HistoryTable.QueryByPrimaryKey(notifyObject.Data);
                    break;
            }


            var notifyObj = new TriggerNotification() {
                Pid = npgsqlNotificationEventArgs.PID,
                Condition = npgsqlNotificationEventArgs.Condition,
                Schema = notifyObject.Schema,
                Table = notifyObject.Table,
                Action = notifyObject.Action,
                EventData = returnObject
            };
            //Console.WriteLine(Json.ToJson(notifyObj, true));
            _tableNotificationSubject.OnNext(notifyObj);
        }

        private ObjectTable GetTable(string tableName)
        {
            var connstr = ConnectionString.Build(_connectionBuilder).WithTable(tableName);
            return new ObjectTable(connstr).NotTyped();
        }


    }

    public class TriggerListener<T> : TriggerListener
    {


        public new IObservable<TriggerNotification<T>> Notifications()
        {

            return _tableNotificationSubject.Select(n => {
                try
                {
                    return n.To<T>();
                } catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return null;
            }).AsObservable();
        }

        public TriggerListener(ConnectionStringBuilder connectionBuilder) : base(connectionBuilder)
        {
        }

        public new TriggerListener<T> ListenOnTablesFqdn(params string[] fqdnTableNames)
        {
            foreach (var tableName in fqdnTableNames)
            {
                if (!tableName.Contains('.'))
                    throw new Exception($"TableName '{tableName}' is not a Full Qualified Name!");

                _listenOnTables.Add(tableName);
            }

            return this;
        }

        public new TriggerListener<T> ListenOnTables(params string[] tableNames)
        {
            foreach (var tableName in tableNames)
            {
                string _schema = "*";
                if (!String.IsNullOrWhiteSpace(_connectionBuilder.GetConnection().SchemaName))
                {
                    _schema = _connectionBuilder.GetConnection().SchemaName;
                }
                _listenOnTables.Add($"{_schema}.{tableName}");
            }

            return this;
        }

    }
}
