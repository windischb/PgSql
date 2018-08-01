using System.Linq;
using doob.PgSql;
using doob.PgSql.Clauses;
using Xunit;

namespace PgSql.Tests
{
    public class TypeTests
    {

        private Schema GetSchema()
        {
            var conbuilder = ConnectionString.Build()
                .WithCredential("postgres","postgres")
                .WithDatabase("Tests")
                .WithSchema("TypeTests");


            return new Schema(conbuilder).CreateIfNotExists();
        }

        [Fact]
        public void Test1() {

            
            
            var tbd = Build.TableDefinition<TypeTestModel>();

            var table = GetSchema().CreateTable("Test1", tbd);
            var model = new TypeTestModel();
            model.String = "Testing...";

            var statement = doob.PgSql.Statements.Insert.Into("TestTable")
                .AddColumnsFromTableDefinition(tbd)
                .AddValuesFromObject(model)
                .AddClause(Returning.Columns(tbd.PrimaryKeys().Select(p => p.GetNameForDb()).ToArray()));

            var sql = statement.GetSqlCommand(tbd);



           
            

            table.Insert(model);

        }

        [Fact]
        public void Test2()
        {
            var tbd = Build.TableDefinition<TypeTestModel>();

            var table = GetSchema().CreateTable("Test1", tbd);
            var model = new TypeTestModel();
            model.String = "Testing...";

            var statement = doob.PgSql.Statements.Insert.Into("TestTable")
                .AddColumnsFromTableDefinition(tbd)
                .AddValuesFromObject(model)
                .AddClause(Returning.Columns(tbd.PrimaryKeys().Select(p => p.GetNameForDb()).ToArray()));

            var sql = statement.GetSqlCommand(tbd);



            table.Insert(model);


            var executor = new PgSqlExecuter(table.GetConnectionString());

            
            var resp = executor.ExecuteReader<TypeTestModel>($"SELECT * FROM {table}");


        }

       
    }
}
