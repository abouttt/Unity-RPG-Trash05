using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using AYellowpaper.SerializedCollections;

[CreateAssetMenu(menuName = "Settings/Scene Settings", fileName = "SceneSettings")]
public class SceneSettings : SingletonScriptableObject<SceneSettings>
{
    [Serializable]
    public class Settings
    {
        [field: SerializeField]
        public AssetLabelReference[] AddressableLabels { get; private set; }

        [field: SerializeField]
        public bool ReloadSceneWhenNoResources { get; private set; }

        [field: SerializeField]
        public Sprite Background { get; private set; }

        [field: SerializeField]
        public AudioClip BGM { get; private set; }
    }

    public Settings this[string sceneAddress] => _settings[sceneAddress];

    [SerializeField, SerializedDictionary("Scene Address", "Settings")]
    private SerializedDictionary<string, Settings> _settings;
}
