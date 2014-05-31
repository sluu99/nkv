using System;

namespace Nkv.Interfaces
{
    public interface INkvSession : IDisposable
    {
        void CreateTable<T>() where T : Entity;
        T Select<T>(string key) where T : Entity;
        T[] SelectPrefix<T>(string prefix) where T : Entity;
        T[] SelectMany<T>(params string[] keys) where T : Entity;
        T[] SelectAll<T>(long skip, int take) where T : Entity;
        void Insert<T>(T entity) where T : Entity;
        void Update<T>(T entity) where T : Entity;
        void Delete<T>(T entity) where T : Entity;
        void ForceDelete<T>(T entity) where T : Entity;
        void Lock<T>(T entity) where T : Entity;
        void Unlock<T>(T entity) where T : Entity;
        long Count<T>() where T : Entity;
    }
}
