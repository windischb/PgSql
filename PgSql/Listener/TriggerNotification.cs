using System;
using System.Collections.Generic;
using doob.PgSql.ExtensionMethods;

namespace doob.PgSql.Listener
{

  
    public class TriggerNotification<T> : EventArgs
    {
        public int Pid { get; set; }
        public string Condition { get; set; }
        public string Schema { get; set; }
        public string Table { get; set; }
        public string Action { get; set; }
        public T EventData { get; set; }

        
    }

    public class TriggerNotification : TriggerNotification<Dictionary<string, object>>
    {
        public TriggerNotification<T> To<T>()
        {
            var notify = new TriggerNotification<T>();
            notify.Pid = Pid;
            notify.Condition = Condition;
            notify.Table = Table;
            notify.Action = Action;
            notify.Schema = Schema;
            notify.EventData = EventData.CloneTo<T>(); // JSON.ToObject<T>(JSON.ToJson(EventData));
            return notify;
        }
    }

}
