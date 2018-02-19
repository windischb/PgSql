using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using doob.PgSql.Attributes;
using doob.PgSql.ExtensionMethods;

namespace doob.PgSql
{

    public class ConnectionString
    {
        [ConnectionStringMember]
        public string ServerName
        {
            get {

                if (String.IsNullOrWhiteSpace(_serverName))
                    ServerName = "localhost";

                return _serverName;
            }
            internal set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    _serverName = "localhost";
                }
                else
                {
                    //var serv = value.Split('.');
                    _serverName = value;// $"{serv.First().ToUpper()}.{String.Join(".", serv.Skip(1))}".Trim('.').Trim();
                }
            }
        }
        private string _serverName;

        [ConnectionStringMember]
        public int Port
        {
            get
            {
                if (_port == 0)
                {
                    Port = 0;
                }
                return _port;
            }
            internal set { _port = value != 0 ? value : 5432; }
        }
        private int _port;

        [ConnectionStringMember]
        public string UserName
        {
            get { return _username; }
            internal set { _username = value; }
        }
        private string _username;

        [ConnectionStringMember]
        public string Password
        {
            get { return _password; }
            set { _password = String.IsNullOrWhiteSpace(value) ? null : value; }
        }
        private string _password;

        [ConnectionStringMember("Database")]
        public string DatabaseName
        {
            get
            {
                return _databaseName;
            }
            internal set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    _databaseName = null;
                }
                else
                {
                    _databaseName = value;
                }
            }
        }
        private string _databaseName;


        [ConnectionStringMember("Schema")]
        public string SchemaName
        {
            get {
                if (String.IsNullOrWhiteSpace(_schemaName))
                    return "public";

                return _schemaName;
            }
            internal set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    _schemaName = null;
                }
                else
                {
                    _schemaName = value;
                }
            }
        }
        private string _schemaName;

        [ConnectionStringMember("Table")]
        public string TableName
        {
            get { return _tableName; }
            internal set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    _tableName = null;
                }
                else
                {
                    _tableName = value;
                }
            }
        }
        private string _tableName;


        public ConnectionString() : this(null)
        {

        }

        public ConnectionString(string connectionString)
        {

            if (!String.IsNullOrWhiteSpace(connectionString))
            {
                var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var s in connectionString.Split(';'))
                {
                    var sp = s.Split('=');
                    dict.Add(sp[0].Trim(), sp[1].Trim());
                }

                var props = this.GetType().GetTypeInfo()
                    .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (var propertyInfo in props)
                {
                    var attr = propertyInfo.GetCustomAttribute<ConnectionStringMemberAttribute>();
                    if (attr != null)
                    {
                        var name = propertyInfo.Name;
                        if (!String.IsNullOrWhiteSpace(attr.Name))
                        {
                            name = attr.Name;
                        }

                        if (dict.ContainsKey(name))
                        {
                            var val = dict[name];
                            propertyInfo.SetValue(this, Convert.ChangeType(val, propertyInfo.PropertyType));
                        }
                    }
                }
            }
        }


        public static ConnectionStringBuilder Build()
        {
            return new ConnectionStringBuilder();
        }

        public static ConnectionStringBuilder Build(string connectionstring)
        {
            return new ConnectionStringBuilder(connectionstring);
        }

        public static ConnectionStringBuilder Build(ConnectionString connection)
        {
            return new ConnectionStringBuilder(connection.ToString(true));
        }

       
        public ConnectionString Clone()
        {
            return new ConnectionString(this.ToString(true));
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool includeSecret)
        {

            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            if (includeSecret)
                flags = flags | BindingFlags.NonPublic;

            var props = this.GetType().GetTypeInfo()
                .GetProperties(flags);

            var parameters = new List<string>();
            foreach (var propertyInfo in props)
            {
                var attr = propertyInfo.GetCustomAttribute<ConnectionStringMemberAttribute>();
                if (attr != null)
                {
                    var name = propertyInfo.Name;
                    if (!String.IsNullOrWhiteSpace(attr.Name))
                    {
                        name = attr.Name;
                    }
                    var val = propertyInfo.GetValue(this);
                    if (val != null)
                    {
                        var def = propertyInfo.GetDefaultValue();
                        if (!val.Equals(def))
                        {
                            if (!String.IsNullOrWhiteSpace(val?.ToString()))
                            {
                                parameters.Add($"{name}={val}");
                            }
                        }

                    }

                }
            }

            return String.Join(";", parameters.OrderBy(p => p));
        }



    }
}
