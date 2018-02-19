using doob.PgSql.ExtensionMethods;
using doob.PgSql.Interfaces;

namespace doob.PgSql.Clauses
{
    public class GroupByName : IGroupByClauseItem
    {
        public string Name { get; set; }

        private GroupByName() { }

        public GroupByName(string name)
        {
            Name = name.ClearString();
        }
       
        public override string ToString()
        {
            return $"\"{Name}\"";
        }
    }
}
