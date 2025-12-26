using UnityEngine;
using System.Collections.Generic;

public class ObjectPoolManager : MonoBehaviour
{
    private static ObjectPoolManager instance;
    public static ObjectPoolManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("ObjectPoolManager");
                instance = go.AddComponent<ObjectPoolManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private readonly Dictionary<GameObject, ObjectPool<GameObject>> pools = new();
    private readonly Dictionary<GameObject, GameObject> instanceToPrefab = new();
    private readonly Queue<GameObject> containerPool = new();
    private readonly HashSet<GameObject> activeContainers = new();

    [SerializeField] private Transform poolRoot;
    [SerializeField] private Transform containerRoot;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        poolRoot = new GameObject("PooledObjects").transform;
        poolRoot.SetParent(transform);

        containerRoot = new GameObject("Containers").transform;
        containerRoot.SetParent(transform);
    }

    public GameObject GetFromPool(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!pools.ContainsKey(prefab))
            CreatePool(prefab);

        var pool = pools[prefab];
        GameObject obj = null;

        while (obj == null)
        {
            obj = pool.Get();
            if (obj == null)
            {
                obj = Instantiate(prefab, poolRoot);
            }
        }

        obj.transform.SetParent(parent);
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.transform.localScale = Vector3.one;
        obj.SetActive(true);
        instanceToPrefab[obj] = prefab;
        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        if (obj == null) return;

        if (instanceToPrefab.TryGetValue(obj, out GameObject prefab) && pools.ContainsKey(prefab))
        {
            obj.SetActive(false);
            obj.transform.SetParent(poolRoot);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            pools[prefab].Return(obj);
        }
    }

    public GameObject GetContainer(string name, Vector3 position, Transform parent)
    {
        GameObject container;

        if (containerPool.Count > 0)
        {
            container = containerPool.Dequeue();
            while (container == null && containerPool.Count > 0)
            {
                container = containerPool.Dequeue();
            }
        }
        else
        {
            container = null;
        }

        if (container == null)
        {
            container = new GameObject();
        }

        container.name = name;
        container.transform.SetParent(parent);
        container.transform.SetPositionAndRotation(position, Quaternion.identity);
        container.transform.localScale = Vector3.one;
        container.SetActive(true);
        activeContainers.Add(container);

        return container;
    }

    public void ReturnContainer(GameObject container)
    {
        if (container == null) return;

        // First return all children to their pools
        List<GameObject> children = new();
        foreach (Transform child in container.transform)
            children.Add(child.gameObject);

        foreach (var child in children)
            ReturnToPool(child);

        // Then return container to container pool
        container.SetActive(false);
        container.transform.SetParent(containerRoot);
        activeContainers.Remove(container);
        containerPool.Enqueue(container);
    }

    public void PreWarmPool(GameObject prefab, int count)
    {
        if (!pools.ContainsKey(prefab))
            CreatePool(prefab);

        pools[prefab].PreWarm(count);
    }

    public void PreWarmContainers(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject container = new GameObject("Container");
            container.SetActive(false);
            container.transform.SetParent(containerRoot);
            containerPool.Enqueue(container);
        }
    }

    public void ClearAllPools()
    {
        foreach (var pool in pools.Values)
            pool.Clear();

        pools.Clear();
        instanceToPrefab.Clear();
        containerPool.Clear();
        activeContainers.Clear();
    }

    public void ClearPool(GameObject prefab)
    {
        if (pools.TryGetValue(prefab, out var pool))
        {
            pool.Clear();
            pools.Remove(prefab);
        }
    }

    private void CreatePool(GameObject prefab)
    {
        ObjectPool<GameObject> pool = new(
            createFunc: () =>
            {
                GameObject obj = Instantiate(prefab, poolRoot);
                obj.SetActive(false);
                return obj;
            },
            onGet: null,
            onReturn: null,
            onDestroy: null
        );

        pools[prefab] = pool;
    }
}

