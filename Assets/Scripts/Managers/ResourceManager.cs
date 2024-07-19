using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using AYellowpaper.SerializedCollections;
using Object = UnityEngine.Object;

public sealed class ResourceManager : SingletonBehaviour<ResourceManager>
{
    public int ResourceCount => _resources.Count;

    [SerializeField, ReadOnly, SerializedDictionary("Key", "Resource")]
    private SerializedDictionary<string, Object> _resources = new();

    private ResourceManager() { }

    protected override void Init()
    {
        base.Init();

        Addressables.InitializeAsync();
    }

    protected override void Dispose()
    {
        base.Dispose();

        Clear();
    }

    public T Load<T>(string key) where T : Object
    {
        if (_resources.TryGetValue(key, out var resource))
        {
            return resource as T;
        }

        return null;
    }

    public void LoadAsync<T>(string key, Action<T> callback = null) where T : Object
    {
        if (_resources.TryGetValue(key, out var resource))
        {
            callback?.Invoke(resource as T);
        }
        else
        {
            Addressables.LoadAssetAsync<T>(key).Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    if (_resources.ContainsKey(key))
                    {
                        Addressables.Release(op);
                    }
                    else
                    {
                        _resources.Add(key, op.Result);
                    }

                    var resource = _resources[key];
                    callback?.Invoke(resource as T);
                }
                else
                {
                    Debug.LogWarning($"[ResourceManager/LoadAsync] Failed to load asset with key : {key}");
                }
            };
        }
    }

    public void LoadAllAsync<T>(string label, Action<T[]> callback = null) where T : Object
    {
        Addressables.LoadResourceLocationsAsync(label, typeof(T)).Completed += op =>
        {
            if (op.Status != AsyncOperationStatus.Succeeded || op.Result.Count == 0)
            {
                Debug.LogWarning($"[ResourceManager/LoadAllAsync] Failed to load asset with label : {label}");
            }
            else
            {
                int index = 0;
                int loadedCount = 0;
                int totalCount = op.Result.Count;
                var resources = new T[totalCount];

                foreach (var result in op.Result)
                {
                    LoadAsync<T>(result.PrimaryKey, resource =>
                    {
                        resources[index++] = resource;
                        if (++loadedCount == totalCount)
                        {
                            callback?.Invoke(resources);
                        }
                    });
                }
            }
        };
    }

    public GameObject Instantiate(string key, Transform parent = null, bool pooling = false)
    {
        var prefab = Load<GameObject>(key);
        return pooling ? PoolManager.Instance.Pop(prefab, parent) : Instantiate(prefab, parent);
    }

    public T Instantiate<T>(string key, Transform parent = null, bool pooling = false) where T : Component
    {
        var go = Instantiate(key, parent, pooling);
        return go.GetComponent<T>();
    }

    public void InstantiateAsync(string key, Action<GameObject> callback = null, Transform parent = null, bool pooling = false)
    {
        LoadAsync<GameObject>(key, prefab =>
        {
            var go = pooling ? PoolManager.Instance.Pop(prefab, parent) : Instantiate(prefab, parent);
            callback?.Invoke(go);
        });
    }

    public void InstantiateAsync<T>(string key, Action<T> callback = null, Transform parent = null, bool pooling = false) where T : Component
    {
        InstantiateAsync(key, go => callback?.Invoke(go.GetComponent<T>()), parent, pooling);
    }

    public void Destroy(GameObject go)
    {
        if (go == null)
        {
            return;
        }

        if (PoolManager.Instance.Push(go))
        {
            return;
        }

        Object.Destroy(go);
    }

    public void Clear()
    {
        foreach (var kvp in _resources)
        {
            Addressables.Release(kvp.Value);
        }

        _resources.Clear();
        Resources.UnloadUnusedAssets();
    }
}
