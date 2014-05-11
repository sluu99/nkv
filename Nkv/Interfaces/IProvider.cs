using System.Data;

namespace Nkv.Interfaces
{
    public interface IProvider
    {
        /// <summary>
        /// Provide a connection that can be disposed once the operation is over
        /// </summary>
        /// <returns></returns>
        IDbConnection GetConnection();

        /// <summary>
        /// Escape an object name
        /// </summary>
        string Escape(string x);

        string[] GetCreateTableQueries(string tableName);

        string GetSelectQuery(string tableName, out string keyParamName);

        string GetDeleteQuery(string tableName, out string keyParamName, out string timestampParamName);

        string GetInsertQuery(string tableName, out string keyParamName, out string valueParamName);

        string GetUpdateQuery(string tableName, out string keyParamName, out string valueParamName, out string timestampParamName);

        string GetCountQuery(string tableName);

        IDbDataParameter CreateParameter(string name, SqlDbType type, object value, int size = 0);
    }
}
