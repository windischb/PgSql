using System;
using System.Collections.Generic;
using System.Text;
using doob.PgSql;
using doob.PgSql.Clauses;
using Xunit;
using Xunit.Abstractions;

namespace PgSql.Tests
{
    public class WhereBuilderTests
    {

        private readonly ITestOutputHelper _output;

        public WhereBuilderTests(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Fact]
        public void Test1()
        {
            var tbd = Build.TableDefinition<TypeTestModel>();


            var q = Where.Create<TypeTestModel>().Not().Eq(t => t.String, "test"); //.And().Not().Eq(t => t.String, "test2");
            var comm = q.GetSqlCommand(tbd).CommandAsPlainText();

            _output.WriteLine(comm);
        }
    }
}
