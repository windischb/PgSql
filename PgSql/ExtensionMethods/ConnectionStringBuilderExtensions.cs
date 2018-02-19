using doob.PgSql.Listener;

namespace doob.PgSql.ExtensionMethods
{
    public static class ConnectionStringBuilderExtensions
    {
        public static TriggerListener BuildTriggerListener(this ConnectionStringBuilder connectionStringBuilder)
        {
            var tel = new TriggerListener(connectionStringBuilder);
            return tel;
        }

        public static TriggerListener<T> BuildTriggerListener<T>(this ConnectionStringBuilder connectionStringBuilder)
        {
            var tel = new TriggerListener<T>(connectionStringBuilder);
            return tel;
        }

        public static EventTriggerListener BuildEventTriggerListener(this ConnectionStringBuilder connectionStringBuilder)
        {
            var tel = new EventTriggerListener(connectionStringBuilder);
            return tel;
        }
    }
}
