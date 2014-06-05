using System;

namespace Nkv
{
    public interface INkvSession : IDisposable
    {
        void Init<T>() where T : Entity;
        void Init(string tableName);

        T Select<T>(string tableName, string key) where T : Entity;
        T Select<T>(string key) where T : Entity;

        T[] SelectPrefix<T>(string tableName, string prefix) where T : Entity;
        T[] SelectPrefix<T>(string prefix) where T : Entity;

        T[] SelectMany<T>(string tableName, string[] keys) where T : Entity;
        T[] SelectMany<T>(string[] keys) where T : Entity;

        T[] SelectAll<T>(string tableName, long skip, int take) where T : Entity;
        T[] SelectAll<T>(long skip, int take) where T : Entity;
        
        void Insert<T>(string tableName, T entity) where T : Entity;
        void Insert<T>(T entity) where T : Entity;
        
        void Update<T>(string tableName, T entity) where T : Entity;
        void Update<T>(T entity) where T : Entity;
        
        void Delete<T>(string tableName, T entity) where T : Entity;
        void Delete<T>(T entity) where T : Entity;
        
        void ForceDelete<T>(string tableName, T entity) where T : Entity;
        void ForceDelete<T>(T entity) where T : Entity;
        
        void Lock<T>(string tableName, T entity) where T : Entity;
        void Lock<T>(T entity) where T : Entity;
        
        void Unlock<T>(string tableName, T entity) where T : Entity;
        void Unlock<T>(T entity) where T : Entity;
        
        long Count(string tableName);
        long Count<T>() where T : Entity;
    }
}
