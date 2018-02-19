namespace doob.PgSql.Clauses
{
    public class SetValueItem
    {
        public string _key { get; set; }
        public object _value { get; private set; }

        public SetValueItem(string key, object value)
        {
            _key = key;
            _value = value;
        }
        public SetValueItem(object value)
        {
            _value = value;
        }

    }
}
