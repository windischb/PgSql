using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using doob.PgSql;
using doob.PgSql.Tables;
using Xunit;

namespace PgSql.Tests
{
    public class LinqTests
    {

        private Schema GetSchema()
        {
            var conbuilder = ConnectionString.Build()
                .WithCredential("postgres", "postgres")
                .WithDatabase("Tests")
                .WithSchema("LinqTests");


            return new Schema(conbuilder).CreateIfNotExists();
        }


        [Fact]
        public void Simple1()
        {

            //GetSchema().TableDrop("Simple1");
            var tbl = GetSchema().CreateIfNotExists().CreateTable<TypeTestModel>("Simple1");


            var address = new Address("1030", "Vienna");
            address.Position = 27;

            var tm = new TypeTestModel();
            tm.String = "TestString";
            tm.Address = address;
            tm.Strings.Add("s1");
            tm.Strings.Add("S2");
            tbl.Insert(tm);

            //var query = tbl.Where.Eq(t => t.Address.Zip, "123");
            //var comm = query.GetSqlCommand(tbl.TableDefinition).CommandAsPlainText();

            //var q = (IQueryable<TypeTestModel>)PgSqlQueryFactory.Queryable<TypeTestModel>(tbl);

            //var lq = tbl.Queryable().Where(t => t.Address == address).ToList();

            //var lq1 = tbl.Queryable().Where(t => t.Address.Zip == "123" && t.String == "TestString").Where(t2 => t2.Address.Position == 99).ToList();


        }
    }
}
