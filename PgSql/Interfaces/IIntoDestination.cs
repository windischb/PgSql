namespace doob.PgSql.Interfaces
{
    public interface IIntoDestination : ISQLCommand
    {
        string DestinationType { get; }
    }
}
