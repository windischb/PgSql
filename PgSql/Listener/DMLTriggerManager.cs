using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using doob.PgSql.Tables;

namespace doob.PgSql.Listener
{
    public static class DMLTriggerManager
    {
        private static ConcurrentDictionary<string, DMLTriggerListener> _listeners = new ConcurrentDictionary<string, DMLTriggerListener>();



        internal static TableListener GetTiggerListener(ITable table, NotificationSharedBy sharedBy)
        {
            switch (sharedBy)
            {
                case NotificationSharedBy.Schema:
                {
                    var key = $"{BuildSchemaKey(table.GetConnectionString())}";
                    var listener = _listeners.GetOrAdd(key, s => new DMLTriggerListener(table.GetSchema()));
                    return new TableListener(listener, table);
                }
                case NotificationSharedBy.Table:
                {
                    var listener = _listeners.GetOrAdd($"{BuildTableKey(table.GetConnectionString())}", s => new DMLTriggerListener(table.GetSchema()));
                    return new TableListener(listener, table);
                }
                case NotificationSharedBy.None:
                {
                    return new TableListener(new DMLTriggerListener(table.GetSchema()), table);
                }
                default:
                {
                    throw new NotImplementedException();
                }
            }

        }

        internal static TypedTableListener<T> GetTypedTiggerListener<T>(ITable table, NotificationSharedBy sharedBy)
        {
            switch (sharedBy)
            {
                
                case NotificationSharedBy.Schema:
                {
                    var listener = _listeners.GetOrAdd($"{BuildSchemaKey(table.GetConnectionString())}", s => new DMLTriggerListener(table.GetSchema()));
                    return new TypedTableListener<T>(listener, table);
                }
                case NotificationSharedBy.Table:
                {
                    var listener = _listeners.GetOrAdd($"{BuildTableKey(table.GetConnectionString())}", s => new DMLTriggerListener(table.GetSchema()));
                    return new TypedTableListener<T>(listener, table);
                }
                case NotificationSharedBy.None:
                {
                    return new TypedTableListener<T>(new DMLTriggerListener(table.GetSchema()), table);
                }
                default:
                {
                    throw new NotImplementedException();
                }
            }
        }


        private static string BuildSchemaKey(ConnectionString connectionString)
        {
            return $"{connectionString.ServerName}|{connectionString.Port}|{connectionString.UserName}|{connectionString.DatabaseName}|{connectionString.SchemaName}";
        }

        private static string BuildTableKey(ConnectionString connectionString)
        {
            return $"{connectionString.ServerName}|{connectionString.Port}|{connectionString.UserName}|{connectionString.DatabaseName}|{connectionString.SchemaName}|{connectionString.TableName}";
        }
    }

    public enum NotificationSharedBy
    {
        Schema,
        Table,
        None
    }
}
