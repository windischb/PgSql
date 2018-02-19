namespace doob.PgSql.Interfaces
{
    public interface IUpdateDestination : ISQLCommand
    {
        string DestinationType { get; }
    }
}
