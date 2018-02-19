namespace doob.PgSql.Listener
{
    public enum ListeningConnectionState
    {
        Closed = 0,
        Open = 1,
        Reconnect = 2,
        Error = 4
    }
}
