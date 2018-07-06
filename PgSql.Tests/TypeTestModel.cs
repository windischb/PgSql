using System;
using System.Collections.Generic;
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

        public Address Address { get; set; } = new Address("123","Home");

        public List<string> Strings { get; set; } = new List<string>();
    }
}
