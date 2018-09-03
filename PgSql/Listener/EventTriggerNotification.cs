using System;
using doob.PgSql.ExtensionMethods;
using doob.PgSql.Helper;
using Reflectensions.ExtensionMethods;

namespace doob.PgSql.Listener
{
    public class EventTriggerNotification
    {
        public string Action { get; set; }
        public string Schema { get; set; }

        private string _identity;

        public string Identity
        {
            get { return _identity; }
            set
            {

                
                var _value = value;
                var isArray = _value.EndsWith("[]");

                if (isArray)
                    _value = _value.TrimToNull("[]");

                var val = Parser.ParseCSVLine(_value, '.');

                _identity = String.Join(".", val);
                if (isArray)
                    _identity = $"{Identity}[]";
                
            }
        }

        public string Type { get; set; }
    }
}