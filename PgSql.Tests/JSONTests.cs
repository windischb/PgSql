using System;
using doob.PgSql;
using doob.PgSql.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace PgSql.Tests
{
    public class JSONTests
    {

        private readonly ITestOutputHelper _output;

        public JSONTests(ITestOutputHelper output)
        {
            this._output = output;
        }


        [Fact]
        public void TestAliasAttribute() {
            
            var tmodel = new TestModel();
            tmodel.Id = Guid.NewGuid().ToString();
            tmodel.Revision = 1;

            var json = Converter.Json.ToJson(tmodel, true);

            _output.WriteLine(json);
        }

        [Fact]
        public void TestAliasAttributeDeserialize() {

            var json = "{\"id_\": \"c837bed4-bdad-4120-8f18-5ce383a8de4e\",\"rev_\": 1,\"worker_id_\": null}";

            var z = Converter.Json.ToObject<TestModel>(json);

            _output.WriteLine(z.Id);

        }
    }

    public class TestModel {

        [PgSqlPrimaryKey]
        [PgSqlColumn("id_")]
        public string Id { get; set; }

        [PgSqlColumn("rev_")]
        public int Revision { get; set; }

        [PgSqlColumn("worker_id_")]
        public string WorkerId { get; set; }

        [PgSqlColumn("topic_name_")]
        public string TopicName { get; set; }

        [PgSqlColumn("retries_")]
        public int? Retries { get; set; }

        [PgSqlColumn("error_msg_")]
        public string ErrorMessage { get; set; }

    }
}
