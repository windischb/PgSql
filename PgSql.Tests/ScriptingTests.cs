using System;
using System.Collections.Generic;
using doob.PgSql;
using Xunit;

namespace PgSql.Tests
{
    public class ScriptingTests
    {
        [Fact]
        public void CreateDatabase() {

            var tbd = new TableDefinition();
            tbd.AddColumn("RequestId", "guid").AsPrimaryKey().DefaultValue(DefaultValues.Guid.New);
            tbd.AddColumn("Reference", "string").CanBeNull();
            tbd.AddColumn("JobName", "string").CanNotBeNull();
            tbd.AddColumn("Data", "object").CanBeNull();
            tbd.AddColumn("Response", "object").CanBeNull();
            tbd.AddColumn("Status", "string").CanNotBeNull();
            tbd.AddColumn("CreatedAt", "datetime").CanNotBeNull();
            tbd.AddColumn("UpdatedAt", "datetime").CanBeNull();

            var conn = ConnectionString.Build()
                .WithCredential("postgres", "postgres")
                .WithDatabase("TestScripting")
                .WithSchema("Test");

            var tbl = new Schema(conn).CreateIfNotExists().CreateTable("Table", tbd, false);

            var dict = new Dictionary<string, object>();
            dict.Add("RequestId", null);
            dict.Add("Reference", null);
            dict.Add("JobName", "test");
            dict.Add("Data", null);
            dict.Add("Response", null);
            dict.Add("Status", "New");
            dict.Add("CreatedAt", DateTime.Now);
            dict.Add("UpdatedAt", null);

            var id = tbl.Insert(dict);
        }
    }
}
