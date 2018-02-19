using System;
using doob.PgSql;
using doob.PgSql.Attributes;

namespace PgSql.Tests
{
    public class TypeTestModel
    {
        [PgSqlPrimaryKey(DefaultValues.Guid.New)]
        public Guid Id { get; set; }

        [PgSqlDefaultValue(DefaultValues.DateTime.Now)]
        public DateTime CreatedAt { get; set; }

        public string String { get; set; }
    }
}
