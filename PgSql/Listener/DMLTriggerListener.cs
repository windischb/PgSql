using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.Helper;
using doob.PgSql.Logging;
using doob.PgSql.Tables;
using Npgsql;

namespace doob.PgSql.Listener
{

    internal class DMLTriggerListener : IDisposable
    {
       

        private static readonly ILog Logger = LogProvider.For<DMLTriggerListener>();

        //protected internal static ConcurrentDictionary<string, DMLTriggerListener> Listeners = new ConcurrentDictionary<string, DMLTriggerListener>();

        private Guid Id { get; } = Guid.NewGuid();
        private readonly Schema _schema;
        private NpgsqlConnection _npgsqlConnection;
        private readonly BehaviorSubject<ListeningConnectionState> _connectionStateChangeSubject;
        private readonly Subject<TriggerNotification> _tableNotificationSubject = new Subject<TriggerNotification>();
        private readonly List<ITable> _listenOnTables = new List<ITable>();
        private readonly object _listenerLock = new object();
        private CancellationTokenSource _tokenSource;
        private ListeningConnectionState _currentConnectionState = ListeningConnectionState.Disconnected;
        private readonly Subject<ConnectionCommand> _command = new Subject<ConnectionCommand>();
        private IDisposable _reconnectSubscription;
        private object _listenOnTablesLock = new object();


        internal IObservable<TriggerNotification> TriggerNotificationKeys(ITable table)
        {
            AddTableToListen(table);
            SendCommand(ConnectionCommand.Connect);
            return _tableNotificationSubject.Where(n => n.Schema.Equals(table.GetConnectionString().SchemaName) && n.Table.Equals(table.GetConnectionString().TableName)).AsObservable();
        }

        internal IObservable<TriggerNotification> TriggerNotificationEntity(ITable table)
        {
            AddTableToListen(table);
            SendCommand(ConnectionCommand.Connect);
            return _tableNotificationSubject
                .Where(n => n.Schema.Equals(table.GetConnectionString().SchemaName) && n.Table.Equals(table.GetConnectionString().TableName))
                .Select(notifyObject =>
                    {
                        if (notifyObject.Action != TriggerAction.Delete)
                        {
                            var foundEntry = GetTable(notifyObject.Table).QueryByPrimaryKey(notifyObject.EventData);
                            if (foundEntry != null)
                            {
                                notifyObject.EventData = foundEntry;
                            }
                            else
                            {
                                notifyObject.Action = notifyObject.Action == TriggerAction.Insert
                                    ? TriggerAction.DeletedInsert
                                    : TriggerAction.DeletedUpdate;
                            }
                            //notifyObject.EventData = GetTable(notifyObject.Table).QueryByPrimaryKey(notifyObject.EventData);
                        }
                        return notifyObject;
                    })
                .AsObservable();
        }

        public IObservable<ListeningConnectionState> ConnectionStateChanges() => _connectionStateChangeSubject.AsObservable();


        protected internal DMLTriggerListener(Schema schema)
        {

            _schema = schema;
            _connectionStateChangeSubject = new BehaviorSubject<ListeningConnectionState>(ListeningConnectionState.New);
            _connectionStateChangeSubject.Subscribe(async state => await OnConnectionStateChanges(state));

            _tokenSource = new CancellationTokenSource();

            InitCommands();
        }


        public void SendCommand(ConnectionCommand command) => _command.OnNext(command);
        private void InitCommands()
        {
            _command.Subscribe(async s => {

                Logger.Debug(() => $"[{Id}] Incomming Command: {s}");
                switch (s)
                {
                    
                    case ConnectionCommand.Connect:
                    case ConnectionCommand.Reconnect:
                    {
                        
                            if (_currentConnectionState == ListeningConnectionState.Connecting || _currentConnectionState == ListeningConnectionState.Connected)
                                return;

                            SetState(ListeningConnectionState.Connecting);
                           
                            Task.Run(() => startListen(), _tokenSource.Token);
                        
                        
                        break;
                    }
                    case ConnectionCommand.Disconnect:
                    {
                        _tokenSource?.Cancel();
                        break;
                    }
                }
            });
        }

        //private void StartListening()
        //{
        //    if(_currentConnectionState == ListeningConnectionState.Open || _currentConnectionState == ListeningConnectionState.Connecting)
        //        return;

        //    //Console.WriteLine($"StartListening => ");
        //    //ConnectionStateChanges().Subscribe(state => Console.WriteLine($"ConnectionState => {state}"));

        //    _reconnectSubscription = _connectionStateChangeSubject.Subscribe(Reconnect);
        //    _schema.CreateTriggerFunction(true);

        //    foreach (var table in _listenOnTables)
        //    {
        //        table.TriggerCreate("Notify-TableEvent", "Notify-TableEvent", true);
        //    }

            

        //    Task.Run(() =>
        //    {
        //        Listen();
        //    });
        //}

        //private void StopListening()
        //{
        //    _tokenSource?.Cancel();
        //}

        private DMLTriggerListener AddTableToListen(params ITable[] tables)
        {
            lock (_listenOnTablesLock)
            {
                foreach (var table in tables)
                {
                    if (_listenOnTables.Contains(table))
                        continue;

                    if (_currentConnectionState == ListeningConnectionState.Connected)
                    {

                        table.TriggerCreate("Notify-TableEvent", "Notify-TableEvent", true);
                    }
                    else
                    {
                        _listenOnTables.Add(table);
                    }

                }
            }
            

            return this;
        }

        
        

        private void SetState(ListeningConnectionState state)
        {
            
            _currentConnectionState = state;
            _connectionStateChangeSubject.OnNext(_currentConnectionState);

        }
        private void startListen()
        {
            
            lock (_listenerLock)
            {
                _npgsqlConnection?.CloseConnection();
                _npgsqlConnection = new DbExecuter(_schema.ConnectionString).BuildNpgSqlConnetion();
                _npgsqlConnection.Notification += _OnEvent;

                try
                {
                    _schema.CreateTriggerFunction(true);

                    lock (_listenOnTablesLock)
                    {
                        foreach (var table in _listenOnTables)
                        {
                            table.TriggerCreate("Notify-TableEvent", "Notify-TableEvent", true);
                        }
                    }
                    

                    _npgsqlConnection.OpenConnection();
                    NpgsqlCommand prepareCommand = _npgsqlConnection.CreateCommand();
                    prepareCommand.CommandText = $"LISTEN \"Notify-TableEvent\";";
                    prepareCommand.ExecuteNonQuery();
                    prepareCommand.Dispose();

                    SetState(ListeningConnectionState.Connected);

                  
                    while (true)
                    {
                        _npgsqlConnection.OpenConnection();
                        var timeout = _npgsqlConnection.Wait(TimeSpan.FromSeconds(3));

                        //var timeout = Task.Run(() =>
                        //{
                        //    try
                        //    {
                        //        return !_npgsqlConnection.Wait(TimeSpan.FromSeconds(5));
                        //    }
                        //    catch (Exception)
                        //    {
                        //        return false;
                        //    }

                        //}, _tokenSource.Token).GetAwaiter().GetResult();

                        if (!_tableNotificationSubject.HasObservers)
                        {
                            break;
                        }

                        if (_tokenSource.Token.IsCancellationRequested)
                        {
                            break;
                        }



                        if (timeout)
                        {

                            Logger.Debug(() => "Sending Keepalive Message...");

                            NpgsqlCommand keepAliveCommand = _npgsqlConnection.CreateCommand();
                            keepAliveCommand.CommandText = "SELECT 1";
                            keepAliveCommand.ExecuteNonQuery();
                            keepAliveCommand.Dispose();
                        }
                    }


                    NpgsqlCommand closeCommand = _npgsqlConnection.CreateCommand();
                    closeCommand.CommandText = $"UNLISTEN \"Notify-TableEvent\";";
                    closeCommand.ExecuteNonQuery();
                    closeCommand.Dispose();
                    SetState(ListeningConnectionState.Disconnected);
                    

                }
                catch (Exception ex)
                {
                    SetState(ListeningConnectionState.Error);
                    Logger.Error(() => $"[{Id}] {ex.Message}");
                }
                finally
                {
                    _npgsqlConnection.CloseConnection();
                    _npgsqlConnection = null;
                }

            }

        }


        private void _OnEvent(object sender, NpgsqlNotificationEventArgs npgsqlNotificationEventArgs)
        {
            
            var notifyObject = JSON.ToObject<NotificationObject>(npgsqlNotificationEventArgs.AdditionalInformation);

            var fqdn = $"{notifyObject.Schema}.{notifyObject.Table}";

            var notify = false;
            foreach (var s in _listenOnTables)
            {
                var tablefqdn = $"{s.GetConnectionString().SchemaName}.{s.GetConnectionString().TableName}";
                if (Wildcard.IsMatch(fqdn, tablefqdn))
                {
                    notify = true;
                    break;
                }

            }

            if (!notify)
                return;

            var notifyObj = new TriggerNotification()
            {
                Pid = npgsqlNotificationEventArgs.PID,
                Condition = npgsqlNotificationEventArgs.Condition,
                Schema = notifyObject.Schema,
                Table = notifyObject.Table,
                Action = notifyObject.Action,
                EventData = notifyObject.Data
            };

            //Dictionary<string, object> returnObject = null;

            //var table = GetTable(notifyObject.Table);
            //switch (notifyObject.Mode)
            //{
            //    case ListenMode.TableEntryPrimaryKeys:
            //        returnObject = notifyObject.Data;
            //        break;
            //    case ListenMode.TableEntry:
            //        if (notifyObject.Action == TriggerAction.Delete) {
            //            returnObject = notifyObject.Data;
            //        } else {
            //            returnObject = table.QueryByPrimaryKey(notifyObject.Data).CloneTo<Dictionary<string, object>>();
            //        }
            //        break;
            //}


            //var notifyObj = new TriggerNotification()
            //{
            //    Pid = npgsqlNotificationEventArgs.PID,
            //    Condition = npgsqlNotificationEventArgs.Condition,
            //    Schema = notifyObject.Schema,
            //    Table = notifyObject.Table,
            //    Action = notifyObject.Action,
            //    EventData = returnObject
            //};

            _tableNotificationSubject.OnNext(notifyObj);
        }

        private ObjectTable GetTable(string tableName)
        {
            var connstr = ConnectionString.Build(_schema.ConnectionString).WithTable(tableName);
            return new ObjectTable(connstr).NotTyped();
        }

       

        #region internal handling

        private async Task OnConnectionStateChanges(ListeningConnectionState stateChangeEventArgs)
        {
            Logger.Info(() => $"[{Id}] State Changed: {stateChangeEventArgs}");

            switch (stateChangeEventArgs)
            {
                case ListeningConnectionState.New:
                {
                    Logger.Info(() => $"[{Id}] New Connection: {this._schema.ConnectionString}");
                    break;
                }
                case ListeningConnectionState.Error:
                {
                    await Task.Delay(TimeSpan.FromSeconds(10));
                    SendCommand(ConnectionCommand.Reconnect);
                    break;
                }
            }
        }

        #endregion



        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _tokenSource?.Cancel();
                _npgsqlConnection.Dispose();

            }

            _disposed = true;
        }

        ~DMLTriggerListener()
        {
            Dispose(false);
        }

       
    }


    public class TableListener
    {
        private ITable _table;
        private DMLTriggerListener _listener;

        internal TableListener(DMLTriggerListener listener, ITable table)
        {
            _listener = listener;
            _table = table;
        }

        public IObservable<TriggerNotification> GetKeys()
        {
            return _listener.TriggerNotificationKeys(_table);
        }

        public IObservable<TriggerNotification> GetEntity()
        {
            return _listener.TriggerNotificationEntity(_table);
        }

        public IObservable<ListeningConnectionState> ConnectionState()
        {
            return _listener.ConnectionStateChanges();
        }

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _listener = null;
                _table = null;

            }

            _disposed = true;
        }

        ~TableListener()
        {
            Dispose(false);
        }

    }

    public class TypedTableListener<T> : TableListener
    {
        internal TypedTableListener(DMLTriggerListener listener, ITable table) : base(listener, table)
        {
        }

        public IObservable<TriggerNotification<T>> GetTypedKeys()
        {
            return GetKeys().Select(notifyObject => notifyObject.To<T>());
        }

        public IObservable<TriggerNotification<T>> GetTypedEntity()
        {
            return GetEntity().Select(notifyObject => notifyObject.To<T>());
        }



        
    }

}
