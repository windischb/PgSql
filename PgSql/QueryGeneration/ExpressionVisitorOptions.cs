using System;
using System.Collections.Generic;
using System.Text;

namespace doob.PgSql.QueryGeneration
{
    public class ExpressionVisitorOptions
    {
        public string TableName { get; set; }

        public Column Column { get; set; }
    }
}
