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
        private Schema Schema;



        public LTreeTests()
        {
            var conbuilder = ConnectionString.Build()
                .WithCredential("postgres", "postgres")
                .WithDatabase("Tests")
                .WithSchema("LTreeTests");

            Schema = new Schema(conbuilder);
            Schema.Drop(true);
            Schema.CreateIfNotExists();

            
        }
        
        [Fact]
        public void BasicTest() {

            var tbd = Build.TableDefinition<ConfigurationItem>();

            var table = Schema.CreateTable("Test1", tbd);

            var cfgi = new ConfigurationItem();
            cfgi.ParentFolder = "";
            cfgi.Name = "test";

            table.Insert(cfgi);

            var cfgi2 = new ConfigurationItem();
            cfgi2.ParentFolder = "";
            cfgi2.Name = "test1";

            table.Insert(cfgi2);


            var cfgi3 = new ConfigurationItem();
            cfgi3.ParentFolder = "HPOO";
            cfgi3.Name = "test1";

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
