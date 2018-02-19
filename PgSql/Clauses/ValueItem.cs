using doob.PgSql.Interfaces;

namespace doob.PgSql.Clauses
{
    public class ValueItem : IValueItem
    {

        public string _key { get; set; }
        public object _value { get; set; }

        public ValueItem(string key, object value)
        {
            _key = key;
            _value = value;
        }
        public ValueItem(object value)
        {
            _value = value;
            
        }

    }
}
