using UnityEngine;

public abstract class Singleton<T> where T : Singleton<T>, new()
{
    private static T s_instance;
    private static readonly object s_lock = new();

    public static T Instance
    {
        get
        {
            lock (s_lock)
            {
                if (s_instance == null)
                {
                    s_instance = new();
                    s_instance.Init();
                }

                return s_instance;
            }
        }
    }

    protected virtual void Init()
    {
    }
}
