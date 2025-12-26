using UnityEngine;
using System;
using System.Collections.Generic;

public class ObjectPool<T> where T : class
{
    private readonly Queue<T> pool = new();
    private readonly Func<T> createFunc;
    private readonly Action<T> onGet;
    private readonly Action<T> onReturn;
    private readonly Action<T> onDestroy;

    public int CountInactive => pool.Count;

    public ObjectPool(Func<T> createFunc, Action<T> onGet = null, Action<T> onReturn = null, Action<T> onDestroy = null, int initialCapacity = 0)
    {
        this.createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
        this.onGet = onGet;
        this.onReturn = onReturn;
        this.onDestroy = onDestroy;

        PreWarm(initialCapacity);
    }

    public T Get()
    {
        T item = pool.Count > 0 ? pool.Dequeue() : createFunc();
        
        // Skip destroyed items (they will be recreated by caller)
        if (item == null) return null;
        
        onGet?.Invoke(item);
        return item;
    }

    public void Return(T item)
    {
        if (item == null) return;
        onReturn?.Invoke(item);
        pool.Enqueue(item);
    }

    public void PreWarm(int count)
    {
        for (int i = 0; i < count; i++)
        {
            T item = createFunc();
            onReturn?.Invoke(item);
            pool.Enqueue(item);
        }
    }

    public void Clear()
    {
        while (pool.Count > 0)
        {
            T item = pool.Dequeue();
            onDestroy?.Invoke(item);
        }
    }
}
