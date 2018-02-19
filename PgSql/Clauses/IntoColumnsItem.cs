using doob.PgSql.ExtensionMethods;
using doob.PgSql.Interfaces;

namespace doob.PgSql.Clauses
{
    public class IntoColumnsItem : ISQLCommand
    {
        public string _name;

        public static IntoColumnsItem Parse(string sqlStatement)
        {
            var field = new IntoColumnsItem();

            field._name = sqlStatement.ClearString();

            return field;
        }

        private IntoColumnsItem()
        {
            
        }
        public IntoColumnsItem(string name)
        {
            var f = IntoColumnsItem.Parse(name);
            _name = f._name;
        }


        public override string ToString()
        {
            return $"\"{_name}\"";
        }

        public PgSqlCommand GetSqlCommand(TableDefinition tableDefinition)
        {
            var sqlCommand = new PgSqlCommand();
            sqlCommand.AppendCommand($"\"{_name}\"");
            return sqlCommand;
        }
    }
}
