using Newtonsoft.Json;
using Nkv.Attributes;
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
        /// Escape an object name
        /// </summary>
        protected abstract string Escape(string x);

        protected abstract string GetInsertQuery(string tableName, out string keyParamName, out string valueParamName);

        protected abstract IDbDataParameter CreateParameter(string name, SqlDbType type, object value, int size = 0);

        /// <summary>
        /// Create table for a data type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public abstract void CreateTable<T>() where T : Entity;


        public virtual void Insert<T>(T entity) where T : Entity
        {
            ValidateEntity(entity);

            string keyParamName;
            string valueParamName;
            string tableName = TableAttribute.GetTableName(typeof(T));
            string query = GetInsertQuery(tableName, out keyParamName, out valueParamName);

            var json = JsonConvert.SerializeObject(entity);
            var keyParam = CreateParameter(keyParamName, SqlDbType.NVarChar, entity.Key, size: 128);
            var valueParam = CreateParameter(valueParamName, SqlDbType.NVarChar, json);

            Action<IDataReader> readerCallback = (reader) =>
            {
                if (!reader.Read())
                {
                    throw new Exception("Unknown error during insertion");
                }

                int i = 0;
                int rowCount = reader.GetInt32(i++);

                if (rowCount != 1)
                {
                    throw new Exception("Insertion validation failed. Row count = " + rowCount.ToString());
                }

                entity.Timestamp = reader.GetDateTime(i++);
            };

            ExecuteReader(query, readerCallback, keyParam, valueParam);
        }


        protected virtual int ExecuteNonQuery(string query, params IDbDataParameter[] parameters)
        {
            using (IDbConnection conn = _connectionProvider.GetConnection())
            {
                conn.Open();                
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;

                    if (parameters != null && parameters.Length > 0)
                    {
                        foreach (var p in parameters)
                        {
                            cmd.Parameters.Add(p);
                        }
                    }

                    return cmd.ExecuteNonQuery();
                }
            }
        }


        protected virtual void ExecuteReader(string query, Action<IDataReader> callback, params IDbDataParameter[] parameters)
        {
            using (IDbConnection conn = _connectionProvider.GetConnection())
            {
                conn.Open();
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;

                    if (parameters != null && parameters.Length > 0)
                    {
                        foreach (var p in parameters)
                        {
                            cmd.Parameters.Add(p);
                        }
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (callback != null)
                        {
                            callback(reader);
                        }
                    }
                }
            }
        }


        private static void ValidateEntity<T>(T entity) where T : Entity
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            if (string.IsNullOrWhiteSpace(entity.Key))
            {
                throw new ArgumentException("Key cannot be null or white space");
            }
        }
    }
}
