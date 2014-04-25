using Nkv.Interfaces;
using System;
using System.Data.SqlClient;

namespace Nkv.Sql
{
    public class SqlConnectionProvider : IConnectionProvider
    {
        private string _connectionString;

        public SqlConnectionProvider(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("SQL Server connection string is required");
            }

            _connectionString = connectionString;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                Database = conn.Database;
            }
        }

        public System.Data.IDbConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public string Database { get; private set; }
    }
}
