using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using doob.PgSql.ExtensionMethods;
using Npgsql;

namespace doob.PgSql.Listener
{
    public abstract class BaseListener
    {
        public IObservable<ListeningConnectionState> ConnectionStateChanges() => _connectionStateChangeSubject.AsObservable();


        protected BaseListener(ConnectionStringBuilder connectionBuilder)
        {
            this._connectionBuilder = ConnectionString.Build(connectionBuilder);
        }


        protected void _startListening(string notifyName)
        {
            _notifyName = notifyName;
            ReconnectSubscription = _connectionStateChangeSubject.Subscribe(Reconnect);
            Task.Run(() =>
            {
                Listen();
            });
        }

        protected void _stopListening()
        {
            ReconnectSubscription?.Dispose();
            _tokenSource.Cancel();
        }

        protected ConnectionStringBuilder _connectionBuilder;
        private readonly Subject<ListeningConnectionState> _connectionStateChangeSubject = new Subject<ListeningConnectionState>();

        private NpgsqlConnection _listeningConnection;

        private readonly object _listenerLock = new object();
        private CancellationTokenSource _tokenSource;

        private string _notifyName;
        private void Listen()
        {
            
            _listeningConnection?.CloseConnection();
            _connectionStateChangeSubject.OnNext(ListeningConnectionState.Reconnect);

            _listeningConnection = new DbExecuter(_connectionBuilder).BuildNpgSqlConnetion();
            _listeningConnection.Notification += _OnEvent;

            lock (_listenerLock)
            {
                //Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: Listen({notificationChannel}) - LOCK");
                bool ErrorOccured = false;
                _tokenSource = new CancellationTokenSource();


                try
                {

                    _listeningConnection.OpenConnection();
                    NpgsqlCommand prepareCommand = _listeningConnection.CreateCommand();
                    prepareCommand.CommandText = $"LISTEN \"{_notifyName}\";";
                    prepareCommand.ExecuteNonQuery();
                    prepareCommand.Dispose();

                    _connectionStateChangeSubject.OnNext(ListeningConnectionState.Open);

                    while (true)
                    {
                        _listeningConnection.OpenConnection();
                        var timeout = Task.Run(() =>
                        {
                            try
                            {

                                return !_listeningConnection.Wait(TimeSpan.FromSeconds(10));
                            }
                            catch (Exception)
                            {
                                return false;
                            }

                        }, _tokenSource.Token).GetAwaiter().GetResult();

                        if (_tokenSource.Token.IsCancellationRequested)
                        {
                            break;
                        }


                        if (timeout)
                        {
                            //Console.WriteLine("Sending Keepalive Message...");
                            NpgsqlCommand keepAliveCommand = _listeningConnection.CreateCommand();
                            keepAliveCommand.CommandText = "SELECT 1";
                            keepAliveCommand.ExecuteNonQuery();
                            keepAliveCommand.Dispose();
                        }
                    }
                    //Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: Listen({notificationChannel}) - AFTER WHILE");


                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Thread.Sleep(500);
                    ErrorOccured = true;
                }

                if (ErrorOccured)
                {
                    _connectionStateChangeSubject.OnNext(ListeningConnectionState.Error);
                }
                else
                {
                    _connectionStateChangeSubject.OnNext(ListeningConnectionState.Closed);
                }
            }

        }

        private IDisposable ReconnectSubscription;
        private void Reconnect(ListeningConnectionState stateChangeEventArgs)
        {
            switch (stateChangeEventArgs)
            {
                case ListeningConnectionState.Error:
                    {
                        _startListening(_notifyName);
                        break;
                    }
            }
        }


        protected abstract void _OnEvent(object sender, NpgsqlNotificationEventArgs npgsqlNotificationEventArgs);
    }
}
