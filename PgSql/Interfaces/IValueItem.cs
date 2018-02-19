namespace doob.PgSql.Interfaces
{
    public interface IValueItem
    {
        string _key { get; }
        object _value { get; set; }
    }
}
