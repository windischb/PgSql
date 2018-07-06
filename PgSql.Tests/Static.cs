using System;
using System.Collections.Generic;
using System.Text;
using doob.PgSql;

namespace PgSql.Tests
{
    public static class Static
    {
        public static Schema GetSchema(string schemaname = null)
        {
            var conbuilder = ConnectionString.Build()
                .WithCredential("postgres", "postgres")
                .WithDatabase("Tests")
                .WithSchema( schemaname);
            
            return new Schema(conbuilder).CreateIfNotExists();
        }
    }
}
