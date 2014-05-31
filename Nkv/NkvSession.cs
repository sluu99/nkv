using Newtonsoft.Json;
using Nkv.Attributes;
using Nkv.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Nkv
{
    public class NkvSession : INkvSession
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

        public void CreateTable<T>() where T : Entity
        {
            var tableName = TableAttribute.GetTableName(typeof(T));
            var queries = Provider.GetCreateTableQueries(tableName);

            foreach (var query in queries)
            {
                ExecuteNonQuery(query);
            }
        }

        public long Count<T>() where T : Entity
        {
            var tableName = TableAttribute.GetTableName(typeof(T));
            var query = Provider.GetCountQuery(tableName);
            long count = 0;

            Action<IDataReader> readerCallback = (reader) =>
            {
                if (!reader.Read())
                {
                    throw new DataException("Count query did not return any data");
                }
                count = reader.GetInt64(0);
            };

            ExecuteReader(query, readerCallback);

            return count;
        }

        public T Select<T>(string key) where T : Entity
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key cannot be null or white space");
            }

            string keyParamName;
            string query = Provider.GetSelectQuery(TableAttribute.GetTableName(typeof(T)), out keyParamName);
            var keyParam = Provider.CreateParameter(keyParamName, SqlDbType.NVarChar, key, Entity.MaxKeySize);

            var entities = new List<T>();

            ExecuteReader(query, (r) => ReadEntitiesFromReader(r, entities), keyParam);
            return entities.FirstOrDefault();
        }

        public T[] SelectPrefix<T>(string prefix) where T : Entity
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                throw new ArgumentException("Prefix cannot be null or white space");
            }

            var tableName = TableAttribute.GetTableName(typeof(T));
            string prefixParamName;
            var query = Provider.GetSelectPrefixQuery(tableName, ref prefix, out prefixParamName);
            var prefixParam = Provider.CreateParameter(prefixParamName, SqlDbType.NVarChar, prefix);

            List<T> entities = new List<T>();

            ExecuteReader(query, (r) => ReadEntitiesFromReader(r, entities), prefixParam);

            return entities.ToArray();
        }

        public T[] SelectMany<T>(params string[] keys) where T : Entity
        {
            if (keys == null || keys.Length < 1)
            {
                return new T[0];
            }

            string[] keyParamNames;
            var tableName = TableAttribute.GetTableName(typeof(T));
            string query = Provider.GetSelectManyQuery(tableName, keys.Length, out keyParamNames);

            var keyParams = new IDbDataParameter[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(keys[i]))
                {
                    throw new ArgumentException("One of the keys are null or white spaces");
                }

                keyParams[i] = Provider.CreateParameter(keyParamNames[i], SqlDbType.NVarChar, keys[i], Entity.MaxKeySize);
            }

            var entities = new List<T>();

            ExecuteReader(query, (r) => ReadEntitiesFromReader(r, entities), keyParams);

            return entities.ToArray();
        }

        public T[] SelectAll<T>(long skip, int take) where T : Entity
        {
            if (skip < 0)
            {
                throw new ArgumentException("Skip cannot be negative");
            }
            if (take < 1)
            {
                throw new ArgumentException("Take must be greater than zero");
            }

            string tableName = TableAttribute.GetTableName(typeof(T));
            string query = Provider.GetSelectAllQuery(tableName, skip, take);
            var entities = new List<T>();


            ExecuteReader(query, (r) => ReadEntitiesFromReader(r, entities));

            return entities.ToArray();
        }

        public void Insert<T>(T entity) where T : Entity
        {
            ValidateEntity(entity);

            var json = JsonConvert.SerializeObject(entity);

            string tableName = TableAttribute.GetTableName(typeof(T));
            string keyParamName;
            string valueParamName;
            string query = Provider.GetInsertQuery(tableName, out keyParamName, out valueParamName);
            var keyParam = Provider.CreateParameter(keyParamName, SqlDbType.NVarChar, entity.Key, size: Entity.MaxKeySize);
            var valueParam = Provider.CreateParameter(valueParamName, SqlDbType.NVarChar, json);

            Action<IDataReader> readerCallback = (reader) =>
            {
                DateTime timestamp;
                long version;
                ValidateReaderResult(reader, 1, string.Format("Error inserting {0} entity with key={1}", tableName, entity.Key), out timestamp, out version);
                entity.Timestamp = timestamp;
                entity.Version = version;
            };

            ExecuteReader(query, readerCallback, keyParam, valueParam);

        }

        public void Update<T>(T entity) where T : Entity
        {
            ValidateEntity(entity);

            string keyParamName;
            string valueParamName;
            string versionParamName;
            string tableName = TableAttribute.GetTableName(typeof(T));
            string query = Provider.GetUpdateQuery(tableName, out keyParamName, out valueParamName, out versionParamName);

            var json = JsonConvert.SerializeObject(entity);
            var keyParam = Provider.CreateParameter(keyParamName, SqlDbType.NVarChar, entity.Key, size: Entity.MaxKeySize);
            var valueParam = Provider.CreateParameter(valueParamName, SqlDbType.NVarChar, json);
            var versionParam = Provider.CreateParameter(versionParamName, SqlDbType.BigInt, entity.Version);

            Action<IDataReader> readerCallback = (reader) =>
            {
                DateTime timestamp;
                long version;
                ValidateReaderResult(reader, 1, string.Format("Error updating {0} entity with key={1}", tableName, entity.Key), out timestamp, out version);

                entity.Timestamp = timestamp;
                entity.Version = version;
            };

            ExecuteReader(query, readerCallback, keyParam, valueParam, versionParam);
        }

        public void Delete<T>(T entity) where T : Entity
        {
            InternalDelete<T>(entity, false);
        }

        public void ForceDelete<T>(T entity) where T : Entity
        {
            InternalDelete<T>(entity, true);
        }

        public void Lock<T>(T entity) where T : Entity
        {
            InternalLockEntity(entity, true);
        }

        public void Unlock<T>(T entity) where T : Entity
        {
            InternalLockEntity(entity, false);
        }


        private void InternalLockEntity<T>(T entity, bool isLock) where T : Entity
        {
            ValidateEntity(entity);

            string tableName = TableAttribute.GetTableName(typeof(T));
            string keyParamName;
            string timestampParamName;
            string query = isLock ?
                Provider.GetLockQuery(tableName, out keyParamName, out timestampParamName) :
                Provider.GetUnlockQuery(tableName, out keyParamName, out timestampParamName);

            var keyParam = Provider.CreateParameter(keyParamName, SqlDbType.NVarChar, entity.Key, Entity.MaxKeySize);
            var timestampParam = Provider.CreateParameter(timestampParamName, SqlDbType.DateTime, entity.Timestamp);

            Action<IDataReader> readerCallback = (reader) =>
            {
                DateTime timestamp;
                long version;

                ValidateReaderResult(
                    reader,
                    1,
                    string.Format("Error {0}locking {1} entity with key={2}", isLock ? "" : "un", tableName, entity.Key),
                    out timestamp,
                    out version
                );

                entity.Timestamp = timestamp;
            };

            ExecuteReader(query, readerCallback, keyParam, timestampParam);
        }

        private void InternalDelete<T>(T entity, bool forceDelete) where T : Entity
        {
            ValidateEntity(entity);

            string tableName = TableAttribute.GetTableName(typeof(T));
            string keyParamName;
            string versionParamName;
            string query = forceDelete ?
                Provider.GetForceDeleteQuery(tableName, out keyParamName, out versionParamName) :
                Provider.GetDeleteQuery(tableName, out keyParamName, out versionParamName);
            var keyParam = Provider.CreateParameter(keyParamName, SqlDbType.NVarChar, entity.Key, Entity.MaxKeySize);
            var versionParam = Provider.CreateParameter(versionParamName, SqlDbType.BigInt, entity.Version);

            Action<IDataReader> readerCallback = (reader) =>
            {
                DateTime timestamp;
                long version;
                ValidateReaderResult(reader, 1, string.Format("Error deleting {0} entity with key={1}", tableName, entity.Key), out timestamp, out version);

                entity.Timestamp = timestamp;
                entity.Version = version;
            };

            ExecuteReader(query, readerCallback, keyParam, versionParam);
        }

        #region Helper methods

        private void ReadEntitiesFromReader<T>(IDataReader reader, List<T> entities) where T : Entity
        {
            while (reader.Read())
            {
                int i = 0;

                var key = reader.GetString(i++);
                var json = reader.GetString(i++);
                var timestamp = reader.GetDateTime(i++);
                var version = reader.GetInt64(i++);

                var entity = JsonConvert.DeserializeObject<T>(json);
                entity.Key = key;
                entity.Timestamp = timestamp;
                entity.Version = version;

                entities.Add(entity);
            }
        }

        private void ValidateReaderResult(IDataReader reader, int expectedRowCount, string errorMessage, out DateTime timestamp, out long version)
        {
            if (!reader.Read())
            {
                throw new DataException("No result from data reader");
            }

            int i = 0;
            int rowCount = reader.GetInt32(i++);
            var ackCode = reader.GetString(i++);
            timestamp = reader.IsDBNull(i++) ? Entity.DefaultTimestamp : reader.GetDateTime(i - 1);
            version = reader.IsDBNull(i++) ? 0 : reader.GetInt64(i - 1);

            if (rowCount != expectedRowCount || !string.Equals(ackCode, "Success", StringComparison.OrdinalIgnoreCase))
            {
                NkvAckCode ackCodeEnum = NkvAckCode.Unknown;
                Enum.TryParse<NkvAckCode>(ackCode, out ackCodeEnum);

                throw new NkvException(errorMessage)
                {
                    RowCount = rowCount,
                    AckCode = ackCodeEnum,
                    Timestamp = timestamp
                };
            }
        }

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
