namespace doob.PgSql.Listener
{
    public enum ListeningConnectionState
    {
        New = -1,
        Disconnected = 0,
        Connected = 1,
        Connecting = 2,
        Error = 4
    }
}
