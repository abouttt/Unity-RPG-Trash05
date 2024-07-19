using System;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

public sealed class PoolManager : SingletonBehaviour<PoolManager>
{
    #region Pool
    [Serializable]
    private class Pool
    {
        public Transform Root { get; private set; }

        [field: SerializeField]
        public GameObject Original { get; private set; }

        [field: SerializeField]
        public int Max { get; private set; }

        [field: SerializeField]
        public int Size { get; private set; }

        private readonly HashSet<GameObject> _actives = new();
        private readonly Stack<GameObject> _deactives = new();

        public Pool(GameObject original, int size, int max)
        {
            Original = original;
            Root = new GameObject($"{original.name}_Root").transform;
            Max = max;

            for (int i = 0; i < size; i++)
            {
                var go = Create();
                if (go == null)
                {
                    break;
                }

                PushToContainer(go);
            }
        }

        public bool Push(GameObject go)
        {
            if (!_actives.Remove(go))
            {
                return false;
            }

            PushToContainer(go);

            return true;
        }

        public GameObject Pop(Transform parent)
        {
            if (!_deactives.TryPop(out var go))
            {
                go = Create();
                if (go == null)
                {
                    return null;
                }
            }

            go.SetActive(true);
            go.transform.SetParent(parent != null ? parent : Root);
            _actives.Add(go);

            return go;
        }

        public void Clear()
        {
            foreach (var go in _actives)
            {
                Destroy(go);
            }

            foreach (var go in _deactives)
            {
                Destroy(go);
            }

            Size = 0;
            _actives.Clear();
            _deactives.Clear();
        }

        private GameObject Create()
        {
            if (Size == Max)
            {
                return null;
            }

            var go = Instantiate(Original);
            go.name = Original.name;
            Size++;

            return go;
        }

        private void PushToContainer(GameObject go)
        {
            go.SetActive(false);
            go.transform.SetParent(Root);
            _deactives.Push(go);
        }
    }
    #endregion

    /// <summary>
    /// If a pool does not exist when the Pop method is called, a pool is automatically created.
    /// </summary>
    public bool AutoCreateWhenPop { get; set; } = true;

    [SerializeField, ReadOnly, SerializedDictionary("Pool Name", "Info")]
    private SerializedDictionary<string, Pool> _pools = new();

    private PoolManager() { }

    protected override void Dispose()
    {
        base.Dispose();

        Clear();
    }

    public void CreatePool(GameObject original, int size = 1, int max = -1)
    {
        if (original == null)
        {
            Debug.LogWarning($"[PoolManager/CreatePool] Original is null.");
            return;
        }

        if (_pools.ContainsKey(original.name))
        {
            Debug.LogWarning($"[PoolManager/CreatePool] {original.name} pool already exist.");
            return;
        }

        var pool = new Pool(original, size, max);
        pool.Root.SetParent(transform);
        _pools.Add(original.name, pool);
    }

    public bool Push(GameObject go)
    {
        if (go == null)
        {
            Debug.LogWarning($"[PoolManager/Push] GameObject is null.");
            return false;
        }

        if (!_pools.ContainsKey(go.name))
        {
            Debug.LogWarning($"[PoolManager/Push] {go.name} pool no exist.");
            return false;
        }

        return _pools[go.name].Push(go);
    }

    public GameObject Pop(GameObject original, Transform parent = null)
    {
        if (original == null)
        {
            Debug.LogWarning($"[PoolManager/Pop] Original is null.");
            return null;
        }

        if (!_pools.TryGetValue(original.name, out var pool))
        {
            if (AutoCreateWhenPop)
            {
                CreatePool(original);
                pool = _pools[original.name];
            }
            else
            {
                Debug.LogWarning($"[PoolManager/Pop] {original.name} pool no exist.");
                return null;
            }
        }

        return pool.Pop(parent);
    }

    public void ClearPool(string poolName)
    {
        if (_pools.TryGetValue(poolName, out var pool))
        {
            pool.Clear();
        }
        else
        {
            Debug.LogWarning($"[PoolManager/ClearPool] {poolName} pool no exist.");
        }
    }

    public void RemovePool(string poolName)
    {
        if (_pools.TryGetValue(poolName, out var pool))
        {
            pool.Clear();
            _pools.Remove(poolName);
            Destroy(pool.Root.gameObject);
        }
        else
        {
            Debug.LogWarning($"[PoolManager/RemovePool] {poolName} pool no exist.");
        }
    }

    public void Clear()
    {
        foreach (var kvp in _pools)
        {
            kvp.Value.Clear();
        }

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        _pools.Clear();
    }
}
