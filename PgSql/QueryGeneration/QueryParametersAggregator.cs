using System.Collections.Generic;

namespace doob.PgSql.QueryGeneration
{
    /// Aggregates parameters of a query while assigning them unique names to use in a raw statement.
    public class QueryParametersAggregator
    {
        //public Dictionary<string, object> Parameters { get; } 

        public List<PgSqlParameter> _Parameters { get; }

        public QueryParametersAggregator()
        {
            //this.Parameters = new Dictionary<string, object>();
            _Parameters = new List<PgSqlParameter>();
        }

        /// Adds an object that is a query parameter and returns the name that got assigned to it.
        public string AddParameter(object parameterValue, Column column = null)
        {
            var par = new PgSqlParameter(column?.DbName, parameterValue);
            par.SetColum(column);
            par.OverrideType = column?.PgType;
            _Parameters.Add(par);

            //var newParameterName = $"p{Parameters.Count + 1}";
            //Parameters[newParameterName] = parameterValue;

            return par.UniqueId;
        }

        public string AddParameter(string column, object value)
        {
            var par = new PgSqlParameter(column, value);
            _Parameters.Add(par);
            return par.UniqueId;
        }
    }
}