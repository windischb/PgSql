using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
using Reflectensions.Helpers;

namespace doob.PgSql.Listener
{

    internal class DMLTriggerListener : IDisposable
    {


        private static readonly ILog Logger = LogProvider.For<DMLTriggerListener>();

        private Guid Id { get; } = Guid.NewGuid();
        private readonly Schema _schema;
        private NpgsqlConnection _npgsqlConnection;
        private readonly BehaviorSubject<ListeningConnectionState> _connectionStateChangeSubject;
        private readonly Subject<TriggerNotification> _tableNotificationSubject = new Subject<TriggerNotification>();
        
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
           return _tableNotificationSubject
               .Where(n => n?.Schema?.Equals(table.GetConnectionString().SchemaName) == true && n?.Table?.Equals(table.GetConnectionString().TableName) == true).AsObservable();
        }

        internal IObservable<TriggerNotification> TriggerNotificationEntity(ITable table)
        {
            AddTableToListen(table);
            SendCommand(ConnectionCommand.Connect);
            return _tableNotificationSubject
                .Where(n => n?.Schema?.Equals(table.GetConnectionString().SchemaName) == true && n?.Table?.Equals(table.GetConnectionString().TableName) == true)
                .Select(notifyObject =>
                    {
                        try
                        {
                            if (notifyObject.Action != TriggerAction.Delete)
                            {
                                if (!notifyObject.Resolved)
                                {
                                    var foundEntry = GetTable(notifyObject.Table).QueryByPrimaryKey(notifyObject.EventData);

                                    if (foundEntry != null)
                                    {
                                        notifyObject.Resolved = true;
                                        notifyObject.EventData = foundEntry;
                                    }
                                    else
                                    {
                                        notifyObject.Action = notifyObject.Action == TriggerAction.Insert
                                            ? TriggerAction.DeletedInsert
                                            : TriggerAction.DeletedUpdate;
                                    }
                                }
                                
                                
                            }
                            
                        }
                        catch (Exception e)
                        {
                            Logger.Error(() => $"[{Id}] {e.Message}");
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
            _command.Subscribe(async s =>
            {

                Logger.Debug(() => $"[{Id}] Incomming Command: {s}");
                switch (s)
                {

                    case ConnectionCommand.Connect:
                    case ConnectionCommand.Reconnect:
                    {

                        if (_currentConnectionState != ListeningConnectionState.Disconnected && _currentConnectionState != ListeningConnectionState.Error)
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

        private readonly List<ITable> _listenOnTables = new List<ITable>();
        private List<ITable> _wouldLikeToListen = new List<ITable>();

        private DMLTriggerListener AddTableToListen(params ITable[] tables)
        {
            try
            {
                lock (_listenOnTablesLock)
                {
                    foreach (var table in tables)
                    {
                        if (_listenOnTables.Contains(table))
                            continue;

                        if (_currentConnectionState == ListeningConnectionState.Connected)
                        {
                            try
                            {
                                table.TriggerCreate("Notify-TableEvent", "Notify-TableEvent", true);
                                _listenOnTables.Add(table);
                            }
                            catch (Exception e)
                            {
                                Logger.Error(e, "AddTableToListen");
                            }
                           
                        }
                        else
                        {
                            if (_wouldLikeToListen.Contains(table))
                                continue;

                            _wouldLikeToListen.Add(table);
                        }
                        
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(() => $"[{Id}] {e.Message}");
                
            }
            
            return this;
        }

        private void RegisterTableTrigger()
        {
            lock (_listenOnTablesLock)
            {
                _wouldLikeToListen = _wouldLikeToListen.Select(t =>
                {

                    try
                    {
                        Logger.Debug(() => $"WouldLikeToListen: {t.GetConnectionString().SchemaName}.{t.GetConnectionString().TableName}");
                        t.TriggerCreate("Notify-TableEvent", "Notify-TableEvent", true);
                        Logger.Debug(() => $"WouldLikeToListen: {t.GetConnectionString().SchemaName}.{t.GetConnectionString().TableName} - Trigger created successfully");
                        this._listenOnTables.Add(t);
                        return null;
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, $"WouldLikeToListen: {t.GetConnectionString().SchemaName}.{t.GetConnectionString().TableName} - error");
                    }

                    return t;
                }).Where(t => t != null).ToList();

            }
        }


        private void SetState(ListeningConnectionState state)
        {
            
            Logger.Debug(() => $"Set State {state}");
            _currentConnectionState = state;
            _connectionStateChangeSubject.OnNext(_currentConnectionState);

        }

        private object listeningLock = new object();
        private async Task startListenAsync(CancellationToken token)
        {

            lock (listeningLock)
            {
                if (_currentConnectionState != ListeningConnectionState.Disconnected && _currentConnectionState != ListeningConnectionState.Error)
                    return;

                SetState(ListeningConnectionState.Connecting);
            }

            IDisposable subscription = null;

            try
            {
                _npgsqlConnection?.CloseConnection();
                var connstr = Build.ConnectionStringBuilder(_schema.GetConnectionString())
                    .WithTcpKeepalive().WithConnectionIdleLifetime(120).GetConnection().ToNpgSqlConnectionString();

                _npgsqlConnection = new NpgsqlConnection(connstr);
                _npgsqlConnection.Notification += _OnEvent;

                
              


                _schema.CreateTriggerFunction(true);

              


                _npgsqlConnection.OpenConnection();
                NpgsqlCommand prepareCommand = _npgsqlConnection.CreateCommand();
                prepareCommand.CommandText = $"LISTEN \"Notify-TableEvent\";";
                prepareCommand.ExecuteNonQuery();
                prepareCommand.Dispose();

                SetState(ListeningConnectionState.Connected);

                lock (_listenOnTablesLock)
                {
                    _wouldLikeToListen = _wouldLikeToListen.Select(t =>
                    {
                        try
                        {
                            t.TriggerCreate("Notify-TableEvent", "Notify-TableEvent", true);
                            return null;
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e, "AddWouldLikeToListen");
                        }

                        return t;
                    }).Where(t => t != null).ToList();

                }

                while (true)
                {
                    try
                    {

                        _npgsqlConnection.OpenConnection();

                        if (_currentConnectionState != ListeningConnectionState.Connected)
                            SetState(ListeningConnectionState.Connected);

                        await _npgsqlConnection.WaitAsync(token);





                        //if (!_tableNotificationSubject.HasObservers)
                        //{
                        //    break;
                        //}

                        if (_tokenSource.Token.IsCancellationRequested)
                        {
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(() => $"[{Id}]InnerConnectionError: {e.Message}, InnerConnectionState: {_npgsqlConnection?.State}");
                        SetState(ListeningConnectionState.Reconnecting);
                        AsyncHelper.RunSync(() => Task.Delay(1000, token));

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
                
                if (_npgsqlConnection != null)
                {
                    _npgsqlConnection.Notification -= _OnEvent;
                    _npgsqlConnection.CloseConnection();
                    _npgsqlConnection = null;
                }
                
            }



        }

        private void startListen()
        {

            lock (_listenerLock)
            {
                _npgsqlConnection?.CloseConnection();
                var connstr = Build.ConnectionStringBuilder(_schema.GetConnectionString())
                    .WithConnectionIdleLifetime(30).GetConnection().ToNpgSqlConnectionString();

                _npgsqlConnection = new NpgsqlConnection(connstr);
                
                _npgsqlConnection.Notification += _OnEvent;

                try
                {
                    _schema.CreateTriggerFunction(true);

                   
                    _npgsqlConnection.OpenConnection();
                    NpgsqlCommand prepareCommand = _npgsqlConnection.CreateCommand();
                    prepareCommand.CommandText = $"LISTEN \"Notify-TableEvent\";";
                    prepareCommand.ExecuteNonQuery();
                    prepareCommand.Dispose();

                    SetState(ListeningConnectionState.Connected);

                    RegisterTableTrigger();

                    while (true)
                    {
                        Logger.Debug(() => "[Listening] wait...");
                        _npgsqlConnection.OpenConnection();
                        var timeout = !_npgsqlConnection.Wait(TimeSpan.FromSeconds(20));

                        if (!_tableNotificationSubject.HasObservers)
                        {
                            Logger.Debug(() => "[Listening] Stop Listening, because of no more Observers...");
                            break;
                        }

                        if (_tokenSource.Token.IsCancellationRequested)
                        {
                            Logger.Debug(() => "[Listening] Stop Listening, because of cancellationRequest...");
                            break;
                        }



                        if (timeout)
                        {

                            Logger.Debug(() => "[Listening] Sending Keepalive Message...");

                            NpgsqlCommand keepAliveCommand = _npgsqlConnection.CreateCommand();
                            keepAliveCommand.CommandText = "SELECT 1";
                            keepAliveCommand.ExecuteNonQuery();
                            keepAliveCommand.Dispose();
                        }
                        else
                        {
                            Logger.Debug(() => "[Listening] Wait again without sending Keepalive Message...");
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
                    Logger.Debug(() => "[Listening] Listening stopped");
                }

            }

        }


        private void _OnEvent(object sender, NpgsqlNotificationEventArgs npgsqlNotificationEventArgs)
        {

            Logger.Debug(() => $"[{Id}][Notification]: {Converter.Json.ToJson(npgsqlNotificationEventArgs, true)}");

            try
            {
                var notifyObject = Converter.Json.ToObject<NotificationObject>(npgsqlNotificationEventArgs.AdditionalInformation);

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

                _tableNotificationSubject.OnNext(notifyObj);
            }
            catch (Exception e)
            {
                Logger.Error(() => $"[{Id}]OnNotification {e.Message}");
            }
           
        }

        private ObjectTable GetTable(string tableName)
        {
            var connstr = ConnectionString.Build(_schema.GetConnectionString()).WithTable(tableName);
            return new ObjectTable(connstr).NotTyped();
        }



        #region internal handling

        private async Task OnConnectionStateChanges(ListeningConnectionState stateChangeEventArgs)
        {
            

            switch (stateChangeEventArgs)
            {
                case ListeningConnectionState.New:
                {
                    Logger.Debug(() => $"[{Id}] New Connection: {this._schema.GetConnectionString()}");
                    break;
                }
                case ListeningConnectionState.Error:
                {
                    Logger.Error(() => $"[{Id}] State Changed: {stateChangeEventArgs}");
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    SendCommand(ConnectionCommand.Reconnect);
                    break;
                }
                case ListeningConnectionState.Disconnected:
                    Logger.Debug(() => $"[{Id}] State Changed: {stateChangeEventArgs}");
                    break;
                case ListeningConnectionState.Connected:
                    Logger.Info(() => $"[{Id}] State Changed: {stateChangeEventArgs}");
                    break;
                case ListeningConnectionState.Connecting:
                    Logger.Debug(() => $"[{Id}] State Changed: {stateChangeEventArgs}");
                    break;
                case ListeningConnectionState.Reconnecting:
                    Logger.Warn(() => $"[{Id}] State Changed: {stateChangeEventArgs}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stateChangeEventArgs), stateChangeEventArgs, null);
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
