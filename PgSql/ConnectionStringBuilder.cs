using System.Net;

namespace doob.PgSql
{
    public class ConnectionStringBuilder
    {
        private readonly ConnectionString _connectionstring;

        public ConnectionStringBuilder()
        {
            _connectionstring = new ConnectionString();
        }

        public ConnectionStringBuilder(string connectionString)
        {
            _connectionstring = new ConnectionString(connectionString);
        }

        public ConnectionStringBuilder(ConnectionString connectionString)
        {
            _connectionstring = new ConnectionString(connectionString.ToString(true));
        }

        public ConnectionStringBuilder WithServerName(string serverName)
        {
            _connectionstring.ServerName = serverName;
            return this;
        }

        public ConnectionStringBuilder WithPort(int? port)
        {
            _connectionstring.Port = port ?? 0;
            return this;
        }

        public ConnectionStringBuilder WithCredential(NetworkCredential credential)
        {
            if (credential == null)
                return this;

            return this.WithCredential(credential.UserName, credential.Password);
        }

        public ConnectionStringBuilder WithCredential(string username, string password)
        {
            _connectionstring.UserName = username;
            _connectionstring.Password = password;

            return this;
        }

        public ConnectionStringBuilder WithDatabase(string database)
        {
            _connectionstring.DatabaseName = database;
            return this;
        }

        public ConnectionStringBuilder WithSchema(string schemaName)
        {
            _connectionstring.SchemaName = schemaName;
            return this;
        }

        public ConnectionStringBuilder WithTable(string tableName)
        {
            _connectionstring.TableName = tableName;
            return this;
        }

        public ConnectionStringBuilder WithApplicationName(string name)
        {
            _connectionstring.ApplicationName = name;
            return this;
        }

        //public ConnectionBuilder WithTableHistoryEnabled(bool enable = true)
        //{
        //    _connectionstring.EnableTableHistory = enable;
        //    return this;
        //}

        public ConnectionStringBuilder Clone()
        {
            return new ConnectionStringBuilder(this);
        }


        //public string ConnectionString()
        //{
        //    return _connectionstring.ToString(true);
        //}

        //public override string ToString()
        //{
        //    return _connectionstring.ToString();
        //}

        public ConnectionString GetConnection()
        {
            return _connectionstring.Clone();
        }

        public static implicit operator ConnectionString(ConnectionStringBuilder builder)
        {
            return builder._connectionstring;
        }

    }
}