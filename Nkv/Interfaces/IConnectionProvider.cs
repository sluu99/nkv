using System.Data;

namespace Nkv.Interfaces
{
    public interface IConnectionProvider
    {
        /// <summary>
        /// Provide a connection that can be disposed once the operation is over
        /// </summary>
        /// <returns></returns>
        IDbConnection GetConnection();
    }
}
