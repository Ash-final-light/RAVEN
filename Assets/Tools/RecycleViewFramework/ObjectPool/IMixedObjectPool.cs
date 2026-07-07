using System;

namespace Tools.RecycleViewFramework.ObjectPool
{
    public interface IMixedObjectPool<T> : IDisposable where T : class
    {
        T Allocate(string typeName);

        void Free(string typeName, T obj);
    }
}
