using System;
using UnityEngine;

public abstract class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
{
    private static T s_instance;

    public static T Instance
    {
        get
        {
            if (s_instance == null)
            {
                var assets = Resources.LoadAll<T>("");
                if (assets == null || assets.Length == 0)
                {
                    throw new Exception($"Could not find any singleton scriptable object instance in the resources : {typeof(T)}");
                }
                else if (assets.Length > 1)
                {
                    Debug.Log($"Multiple instance of the singleton scriptable object found in the resources : {typeof(T)}");
                }

                s_instance = assets[0];
                s_instance.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }

            return s_instance;
        }
    }
}
