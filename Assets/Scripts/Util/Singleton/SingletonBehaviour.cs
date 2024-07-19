using UnityEngine;

public abstract class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    protected bool _isDestroyOnLoad = false;

    private static T s_instance;

    public static T Instance
    {
        get
        {
            if (s_instance == null)
            {
                s_instance = FindObjectOfType<T>();
                if (s_instance == null)
                {
                    var go = new GameObject(typeof(T).Name);
                    s_instance = go.AddComponent<T>();
                }
            }

            return s_instance;
        }
    }

    private void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {
        if (s_instance == null)
        {
            s_instance = this as T;
            if (!_isDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else if (s_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        Dispose();
    }

    protected virtual void Dispose()
    {
        if (s_instance == this)
        {
            s_instance = null;
        }
    }
}
