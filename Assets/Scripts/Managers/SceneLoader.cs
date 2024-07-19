using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public sealed class SceneLoader : SingletonBehaviour<SceneLoader>
{
    public BaseScene CurrentScene => FindObjectOfType<BaseScene>();
    public string NextSceneAddress { get; private set; }
    public bool IsReadyToLoad { get; private set; }
    public bool IsReadyToCompletion { get; private set; }
    public float LoadingProgress { get; private set; }

    private AsyncOperationHandle<SceneInstance> _sceneHandle;

    private SceneLoader() { }

    protected override void Init()
    {
        base.Init();

        Addressables.InitializeAsync();
    }

    protected override void Dispose()
    {
        base.Dispose();

        Clear();

        if (_sceneHandle.IsValid())
        {
            Addressables.Release(_sceneHandle);
        }
    }

    public void ReadyToLoad(string sceneAddress)
    {
        if (string.IsNullOrEmpty(sceneAddress))
        {
            Debug.LogWarning("[SceneLoader/ReadyToLoad] The scene address is empty");
            return;
        }

        if (IsReadyToLoad)
        {
            Clear();
        }

        NextSceneAddress = sceneAddress;
        IsReadyToLoad = true;

        if (SceneManager.GetActiveScene() != SceneManager.GetSceneByBuildIndex(0))
        {
            SceneManager.LoadScene(0);
        }
    }

    public void StartLoad()
    {
        if (IsReadyToLoad)
        {
            StartCoroutine(LoadSceneAsync());
        }
        else
        {
            Debug.LogWarning("[SceneLoader/StartLoad] Not ready to load");
        }
    }

    public void CompleteLoad()
    {
        if (IsReadyToCompletion)
        {
            Clear();
            _sceneHandle.Result.ActivateAsync().allowSceneActivation = true;
        }
        else
        {
            Debug.LogWarning("[SceneLoader/CompleteLoad] Not ready to completion");
        }
    }

    private IEnumerator LoadSceneAsync()
    {
        float timer = 0f;

        _sceneHandle = Addressables.LoadSceneAsync(NextSceneAddress, LoadSceneMode.Single, false);

        while (!_sceneHandle.IsDone)
        {
            yield return null;

            if (_sceneHandle.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogWarning($"[SceneLoader] Failed to load scene : {NextSceneAddress}");
                yield break;
            }

            timer += Time.unscaledDeltaTime;
            if (_sceneHandle.PercentComplete < 0.9f)
            {
                LoadingProgress = Mathf.Lerp(LoadingProgress, _sceneHandle.PercentComplete, timer);
            }
            else
            {
                break;
            }
        }

        if (_sceneHandle.Status == AsyncOperationStatus.Succeeded)
        {
            timer = 0f;

            while (true)
            {
                yield return null;

                timer += Time.unscaledDeltaTime;
                LoadingProgress = Mathf.Lerp(LoadingProgress, 1f, timer);
                if (LoadingProgress >= 1f)
                {
                    IsReadyToCompletion = true;
                    yield break;
                }
            }
        }
    }

    private void Clear()
    {
        NextSceneAddress = string.Empty;
        IsReadyToLoad = false;
        IsReadyToCompletion = false;
        LoadingProgress = 0f;
    }
}
