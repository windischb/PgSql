using System;
using System.Collections.Generic;
using System.Dynamic;
using doob.PgSql.Exceptions;
using Npgsql;

namespace doob.PgSql
{
    public class Server
    {
        private readonly ConnectionStringBuilder _configuration;

        public Server()
        {
            _configuration = ConnectionString.Build().WithDatabase("postgres");
        }
        public Server(string connectionstring)
        {
            _configuration = ConnectionString.Build(connectionstring).WithDatabase("postgres");
        }
        public Server(ConnectionString connection)
        {
            _configuration = ConnectionString.Build(connection).WithDatabase("postgres");
        }
        public Server(ConnectionStringBuilder connectionBuilder)
        {
            _configuration = ConnectionString.Build(connectionBuilder).WithDatabase("postgres");
        }

        public List<Dictionary<string, object>> GetServerSetting()
        {
            var settings = new List<Dictionary<string, object>>();
            foreach (var expandableObject in new DbExecuter(_configuration).ExecuteReader(SQLStatements.GetAllSettings()))
            {
                settings.Add(JSON.ToObject<Dictionary<string, object>>(expandableObject));
            }
            return settings;
        }

        public Database GetDatabase(string databaseName)
        {
            var connstr = ConnectionString.Build(_configuration).WithDatabase(databaseName);
            return new Database(connstr);
        }
        public IEnumerable<string> GetDatabaseList()
        {
            foreach (var expandableObject in new DbExecuter(_configuration).ExecuteReader(SQLStatements.DatabaseListAll()))
            {
                var obj = JSON.ToObject<Dictionary<string, object>>(expandableObject);
                yield return obj["datname"].ToString();
            }
        }


        public bool DatabaseExists(string databaseName)
        {
            return DatabaseExists(databaseName, false);
        }
        public bool DatabaseExists(string databaseName, bool throwIfNotExists)
        {
            return DatabaseExists(databaseName, throwIfNotExists, false);
        }
        public bool DatabaseExists(string database, bool throwIfNotExists, bool throwIfExists)
        {
            if (String.IsNullOrWhiteSpace(database))
                throw new ArgumentNullException(nameof(database));

            var exists = new DbExecuter(_configuration).ExecuteScalar<bool>(SQLStatements.DatabaseExists(database));

            if (!exists && throwIfNotExists)
                throw new DatabaseDoesntExistsException(database);

            if (exists && throwIfExists)
                throw new DatabaseAlreadyExistsException(database);

            return exists;
        }

        public Database CreateDatabase(string database)
        {
            return CreateDatabase(database, true);
        }
        public Database CreateDatabase(string database, bool throwIfAlreadyExists)
        {
            if (String.IsNullOrWhiteSpace(database))
                throw new ArgumentNullException(nameof(database));

            try
            {

                if (!DatabaseExists(database, false, throwIfAlreadyExists))
                {
                    try
                    {
                        new DbExecuter(_configuration).ExecuteNonQuery(SQLStatements.DatabaseCreate(database));
                    }
                    catch (PostgresException pex)
                    {
                        if (throwIfAlreadyExists && pex.SqlState != "42P04")
                            throw;
                    }

                }
                return GetDatabase(database);
            }
            catch (PostgresException pex)
            {
                if (pex.SqlState == "42P04")
                    throw new DatabaseAlreadyExistsException(database);

                throw;
            }
        }

        public void DropDatabase(string databaseName)
        {
            DropDatabase(databaseName, false);
        }
        public void DropDatabase(string databaseName, bool force)
        {
            DropDatabase(databaseName, force, false);
        }

        public void DropDatabase(string databaseName, bool force, bool throwIfNotExists)
        {
            if (String.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentNullException(nameof(databaseName));

            if (force)
                DropDatabaseConnections(databaseName);

            new DbExecuter(_configuration).ExecuteNonQuery(SQLStatements.DatabaseDrop(databaseName, throwIfNotExists));
        }

        public void DropDatabaseConnections(string databaseName)
        {
            if (String.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentNullException(nameof(databaseName));

            new DbExecuter(_configuration).ExecuteNonQuery(SQLStatements.DatabaseConnectionsDrop(databaseName));
        }
        public IEnumerable<object> GetDatabaseConnections(string databaseName)
        {
            if (String.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentNullException(nameof(databaseName));

            foreach (var expandableObject in new DbExecuter(_configuration).ExecuteReader<ExpandoObject>(SQLStatements.DatabaseConnectionsGet(databaseName)))
            {
                yield return expandableObject;
            }

        }

    }
}
