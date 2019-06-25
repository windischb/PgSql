using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using doob.PgSql.Helper;
using Npgsql;
using Reflectensions.Helpers;

namespace doob.PgSql.Listener
{
    public class EventTriggerListener : BaseListener
    {
        public EventTriggerListener(ConnectionStringBuilder connectionBuilder) : base(connectionBuilder)
        {
        }

        public IObservable<EventTriggerNotification> Notifications() => _notificationSubject.AsObservable();

        private readonly Subject<EventTriggerNotification> _notificationSubject = new Subject<EventTriggerNotification>();


        private List<string> _actionsToListen;
        public void StartListening(params string[] actionsToListen)
        {

            if (actionsToListen.Length == 0)
                actionsToListen = new[] { "*" };

            _actionsToListen = actionsToListen.ToList();
            _startListening("XA-Notify-SchemaEvent");
        }

        protected override void _OnEvent(object sender, NpgsqlNotificationEventArgs npgsqlNotificationEventArgs)
        {

            var notifyObject = Converter.Json.ToObject<EventTriggerNotification>(npgsqlNotificationEventArgs.AdditionalInformation);

            if (notifyObject.Schema != _connectionBuilder.GetConnection().SchemaName)
                return;

            var notify = false;
            foreach (var s in _actionsToListen)
            {
                if (Wildcard.IsMatch(notifyObject.Action, s))
                {
                    notify = true;
                    break;
                }

            }

            if (notify)
                _notificationSubject.OnNext(notifyObject);
        }
    }
}
