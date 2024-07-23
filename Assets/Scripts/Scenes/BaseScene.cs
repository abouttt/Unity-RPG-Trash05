using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseScene : MonoBehaviour
{
    [field: SerializeField]
    public string SceneAddress { get; private set; }

    private void Awake()
    {
        if (SceneSettings.Instance[SceneAddress].ReloadSceneWhenNoResources && ResourceManager.Instance.ResourceCount == 0)
        {
            SceneLoader.Instance.ReadyToLoad(SceneAddress);
        }
        else
        {
            Init();
        }
    }

    protected virtual void Init()
    {
        if (FindObjectOfType(typeof(EventSystem)) == null)
        {
            ResourceManager.Instance.InstantiateAsync("EventSystem.prefab");
        }
    }

    protected void InstantiatePackage(string packageName)
    {
        var package = ResourceManager.Instance.Instantiate(packageName);
        package.transform.DetachChildren();
        Destroy(package);
    }
}
