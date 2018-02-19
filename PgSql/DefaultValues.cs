namespace doob.PgSql
{
    public static class DefaultValues
    {
        public static class Guid
        {
            public const string New = "uuid_generate_v1mc()";
        }

        public static class DateTime
        {
            public const string Now = "localtimestamp";
        }

        public static class Serial
        {
            public const string BigSerial = "bigserial";
        }
    }

    
}
