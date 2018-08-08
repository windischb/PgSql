using System;
using System.Collections.Generic;
using doob.PgSql;
using doob.PgSql.Attributes;
using doob.PgSql.CustomTypes;
using doob.PgSql.Tables;
using Newtonsoft.Json.Linq;
using Xunit;

namespace PgSql.Tests
{
    public class LTreeTests
    {
        private Schema GetSchema()
        {
            var conbuilder = ConnectionString.Build()
                .WithCredential("postgres", "postgres")
                .WithDatabase("Tests")
                .WithSchema("LTreeTests");


            return new Schema(conbuilder).CreateIfNotExists();
        }
        
        [Fact]
        public void BasicTest() {

            var tbd = Build.TableDefinition<ConfigurationItem>();

            var table = GetSchema().CreateTable("Test1", tbd);

            var cfgi = new ConfigurationItem();
            cfgi.ParentFolder = "";
            cfgi.Name = "test";

            table.Insert(cfgi);

            var cfgi2 = new ConfigurationItem();
            cfgi2.ParentFolder = "";
            cfgi2.Name = "test1";

            table.Insert(cfgi2);

            var cfgi3 = new ConfigurationItem();
            cfgi3.ParentFolder = "";
            cfgi3.Name = "test";

            table.Insert(cfgi3);
        }
    }

    public class ConfigurationItem
    {
        [PgSqlPrimaryKey(DefaultValues.Guid.New)]
        public Guid Id { get; set; }

        [PgSqlUnique("FullPath")]
        public PgSqlLTree ParentFolder { get; set; }

        [PgSqlUnique("FullPath")]
        public string Name { get; set; }
        public JToken Value { get; set; }

        public string Type { get; set; }
        public string TypeDetail { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<string> Tags { get; set; } = new List<string>();
        public string LastModifiedBy { get; set; }
    }
}
