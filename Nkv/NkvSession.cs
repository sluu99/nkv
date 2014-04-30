using Newtonsoft.Json;
using Nkv.Attributes;
using Nkv.Interfaces;
using System;
using System.Data;

namespace Nkv
{
    public sealed class NkvSession : IDisposable
    {
        internal NkvSession(IProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            Provider = provider;
            Connection = provider.GetConnection();

            if (this.Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }
        }

        private IDbConnection Connection { get; set; }
        private IProvider Provider { get; set; }

        public void CreateTable<T>()
        {
            var tableName = TableAttribute.GetTableName(typeof(T));
            var query = Provider.GetCreateTableQuery(tableName);
            ExecuteNonQuery(query);
        }

        public void Save<T>(T entity) where T : Entity
        {
            ValidateEntity(entity);

            string keyParamName;
            string valueParamName;
            string timestampParamName;
            string tableName = TableAttribute.GetTableName(typeof(T));
            string query = Provider.GetSaveQuery(tableName, out keyParamName, out valueParamName, out timestampParamName);

            var json = JsonConvert.SerializeObject(entity);
            var keyParam = Provider.CreateParameter(keyParamName, SqlDbType.NVarChar, entity.Key, size: 128);
            var valueParam = Provider.CreateParameter(valueParamName, SqlDbType.NVarChar, json);
            var timestampParam = Provider.CreateParameter(timestampParamName, SqlDbType.DateTime, entity.Timestamp);

            Action<IDataReader> readerCallback = (reader) =>
            {
                if (!reader.Read())
                {
                    throw new Exception("Unknown error during insertion");
                }

                int i = 0;
                int rowCount = reader.GetInt32(i++);
                var timestamp = reader.GetDateTime(i++);
                var ackCode = reader.GetString(i++);

                if (rowCount != 1 || !string.Equals(ackCode, "SUCCESS", StringComparison.OrdinalIgnoreCase))
                {
                    throw new NkvException(string.Format("Error saving the entity key={0}", entity.Key))
                    {
                        RowCount = rowCount,
                        AckCode = ackCode,
                        Timestamp = timestamp
                    };
                }

                entity.Timestamp = timestamp;
            };

            ExecuteReader(query, readerCallback, keyParam, valueParam, timestampParam);
        }

        public T Select<T>(string key) where T : Entity
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key cannot be null or white space");
            }

            string keyParamName;
            string query = Provider.GetSelectQuery(TableAttribute.GetTableName(typeof(T)), out keyParamName);
            var keyParam = Provider.CreateParameter(keyParamName, SqlDbType.NVarChar, key, 128);

            T entity = null;

            Action<IDataReader> readerCallback = (reader) =>
            {
                if (reader.Read())
                {
                    int i = 0;
                    var json = reader.GetString(i++);
                    var timestamp = reader.GetDateTime(i++);

                    entity = JsonConvert.DeserializeObject<T>(json);
                    entity.Key = key;
                    entity.Timestamp = timestamp;
                }
            };

            ExecuteReader(query, readerCallback, keyParam);
            return entity;
        }

        public void Delete<T>(T entity) where T : Entity
        {
            ValidateEntity(entity);

            string keyParamName;
            string timestampParamName;
            string query = Provider.GetDeleteQuery(TableAttribute.GetTableName(typeof(T)), out keyParamName, out timestampParamName);
            var keyParam = Provider.CreateParameter(keyParamName, SqlDbType.NVarChar, entity.Key, 128);
            var timestampParam = Provider.CreateParameter(timestampParamName, SqlDbType.DateTime, entity.Timestamp);

            int rowCount = ExecuteNonQuery(query, keyParam, timestampParam);

            if (rowCount != 1)
            {
                throw new Exception("Delete validation failed");
            }
        }

        #region Helper methods

        private int ExecuteNonQuery(string query, params IDbDataParameter[] parameters)
        {
            using (IDbCommand cmd = Connection.CreateCommand())
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


        private void ExecuteReader(string query, Action<IDataReader> callback, params IDbDataParameter[] parameters)
        {
            using (IDbCommand cmd = Connection.CreateCommand())
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

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (Connection != null && Connection.State != ConnectionState.Closed)
            {
                try
                {
                    Connection.Close();
                    Connection.Dispose();
                }
                catch { }
            }
        }

        #endregion
    }
}
