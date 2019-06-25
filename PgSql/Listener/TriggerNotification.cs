using System;
using System.Collections.Generic;
using doob.PgSql.ExtensionMethods;

namespace doob.PgSql.Listener
{


    public class TriggerNotification : EventArgs
    {
        public int Pid { get; set; }
        public string Condition { get; set; }
        public string Schema { get; set; }
        public string Table { get; set; }
        public TriggerAction Action { get; set; }
        public object EventData { get; set; }

        internal bool Resolved { get; set; }

        public TriggerNotification<T> To<T>()
        {
            var notify = new TriggerNotification<T>();
            notify.Pid = Pid;
            notify.Condition = Condition;
            notify.Table = Table;
            notify.Action = Action;
            notify.Schema = Schema;

            notify.EventData = EventData.CloneTo<T>();
            return notify;
        }
    }

    public class TriggerNotification<T> : TriggerNotification
    {
        public new T EventData { get; set; }


    }

    public enum TriggerAction
    {
        Insert,
        Update,
        Delete,
        DeletedInsert,
        DeletedUpdate
    }

}
