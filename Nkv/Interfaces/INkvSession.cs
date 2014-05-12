using System;

namespace Nkv.Interfaces
{
    public interface INkvSession : IDisposable
    {
        void CreateTable<T>() where T : Entity;
        T Select<T>(string key) where T : Entity;
        void Insert<T>(T entity) where T : Entity;
        void Update<T>(T entity) where T : Entity;
        void Delete<T>(T entity) where T : Entity;
        long Count<T>() where T : Entity;
    }
}
