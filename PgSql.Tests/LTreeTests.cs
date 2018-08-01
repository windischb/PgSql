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
          
        }
    }

    public class ConfigurationItem
    {
        [PgSqlPrimaryKey(DefaultValues.Guid.New)]
        public Guid Id { get; set; }

        [PgSqlUnique]
        public PgSqlLTree ParentFolder { get; set; }

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
