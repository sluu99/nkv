using Nkv.Interfaces;
using System;
using System.Data;

namespace Nkv
{
    public abstract class Nkv
    {
        protected IConnectionProvider _connectionProvider;

        public Nkv(IConnectionProvider connectionProvider)
        {
            if (connectionProvider == null)
            {
                throw new ArgumentNullException("connectionProvider");
            }

            _connectionProvider = connectionProvider;
        }

        /// <summary>
        /// Create table for a data type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public abstract void CreateTable<T>() where T : class;

        protected virtual void ExecuteNonQuery(string query)
        {
            using (IDbConnection conn = _connectionProvider.GetConnection())
            {
                conn.Open();
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
