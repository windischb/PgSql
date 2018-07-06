using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using doob.PgSql.Tables;

namespace doob.PgSql.Listener
{
    internal class SubscriptionManager
    {

        private readonly List<EventSub> _subscriptions = new List<EventSub>();

        private readonly Subject<bool> _hasObservers = new Subject<bool>();

        public IObservable<bool> HasObservers => _hasObservers.AsObservable();

        public int ObserverCount { get; private set; } = 0;

        
        private readonly object _lock = new object();
        public TableEventSubscription Add(ITable table, object action, Func<IDisposable> subscription)
        {
            lock (_lock)
            {
                var exists = _subscriptions.FirstOrDefault(t => t.Table == table && t.Action == action);
                if (exists == null)
                {
                    exists = new EventSub(table, action, subscription.Invoke());
                    _subscriptions.Add(exists);
                    ObserverCount++;
                    _hasObservers.OnNext(ObserverCount != 0);
                }
                return new TableEventSubscription(this, exists);
            }
        }


        public void Remove(ITable table, object action)
        {
            lock (_lock)
            {
                var exists = _subscriptions.FirstOrDefault(t => t.Table == table && t.Action == action);
                if (exists == null)
                    return;

                exists.Subscription.Dispose();
                _subscriptions.Remove(exists);
                ObserverCount--;
                _hasObservers.OnNext(ObserverCount != 0);
            }
        }
    }

    internal class EventSub
    {
        public ITable Table { get; set; }
        public object Action { get; set; }
        public IDisposable Subscription { get; set; }


        public EventSub(ITable table, object action, IDisposable subscription)
        {
            Table = table;
            Action = action;
            Subscription = subscription;
        }
    }


    public class TableEventSubscription
    {

        private SubscriptionManager _subscriptionManager { get; set; }
        private EventSub _eventSub { get; set; }

        internal TableEventSubscription(SubscriptionManager subscriptionManager, EventSub eventSub) {
            _subscriptionManager = subscriptionManager;
            _eventSub = eventSub;
        }

        public void Cancel()
        {
            _subscriptionManager.Remove(_eventSub.Table, _eventSub.Action);
        }
    }

}
