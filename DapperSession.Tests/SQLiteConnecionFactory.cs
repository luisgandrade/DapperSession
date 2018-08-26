using System;
using System.Data;
using Microsoft.Data.Sqlite;

namespace DapperSession.Tests {
    public class SQLiteConnecionFactory : IConnectionFactory {

        private readonly string _connectionString = "Data Source=:memory:";
        private readonly Lazy<IDbConnection> _mainConnection;

        internal void SetupDatabase () 
        {
            var createTable =
                @"CREATE TABLE Person(
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name varchar(256) NOT NULL);";
            var seedData =
                @"INSERT INTO Person(name) VALUES ('John');
                  INSERT INTO Person(name) VALUES ('Mary');
                  INSERT INTO Person(name) VALUES ('Jeff');";

            if (_mainConnection.Value.State != ConnectionState.Open)
                _mainConnection.Value.Open ();

            using (var command = _mainConnection.Value.CreateCommand ()) 
            {
                command.CommandText = createTable;
                command.ExecuteNonQuery ();
            }
            using (var command = _mainConnection.Value.CreateCommand ()) 
            {
                command.CommandText = seedData;
                command.ExecuteNonQuery ();
            }
        }

        internal void TearDown () {
            if (_mainConnection.Value.State != ConnectionState.Open)
                _mainConnection.Value.Open ();

            using (var command = _mainConnection.Value.CreateCommand ()) 
            {
                command.CommandText = "DROP TABLE Person;";
                command.ExecuteNonQuery ();
            }
            _mainConnection.Value.Dispose();
        }

        public IDbConnection GetConnection () 
        {
            return _mainConnection.Value;
        }

        public SQLiteConnecionFactory () 
        {
            SQLitePCL.Batteries.Init ();
            _mainConnection = new Lazy<IDbConnection> (() => new SqliteConnection (_connectionString));
        }
    }
}