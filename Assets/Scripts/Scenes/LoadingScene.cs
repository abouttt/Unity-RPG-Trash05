using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class LoadingScene : BaseScene
{
    [SerializeField]
    private string _defaultSceneAddress;

    protected override void Init()
    {
        base.Init();

        SoundManager.Instance.Clear();
        PoolManager.Instance.Clear();
        ResourceManager.Instance.Clear();
        UIManager.Instance.Clear();
        GC.Collect();

        if (!SceneLoader.Instance.IsReadyToLoad)
        {
            SceneLoader.Instance.ReadyToLoad(_defaultSceneAddress);
        }
    }

    private void Start()
    {
        LoadResourcesByLabels(SceneLoader.Instance.StartLoad);
    }

    private void LateUpdate()
    {
        if (SceneLoader.Instance.IsReadyToCompletion)
        {
            SceneLoader.Instance.CompleteLoad();
        }
    }

    private void LoadResourcesByLabels(Action callback)
    {
        var loadResourceLabels = SceneSettings.Instance[SceneLoader.Instance.NextSceneAddress].AddressableLabels;
        if (loadResourceLabels == null || loadResourceLabels.Length == 0)
        {
            callback?.Invoke();
            return;
        }

        int loadedCount = 0;
        int totalCount = loadResourceLabels.Length;

        foreach (var label in loadResourceLabels)
        {
            ResourceManager.Instance.LoadAllAsync<Object>(label.labelString, _ =>
            {
                loadedCount++;
                if (loadedCount == totalCount)
                {
                    callback?.Invoke();
                }
            });
        }
    }
}
