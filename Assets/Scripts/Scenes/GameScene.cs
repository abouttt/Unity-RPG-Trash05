using UnityEngine;

public class GameScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
    }

    private void Start()
    {
        InputManager.Instance.CursorLocked = true;
        SoundManager.Instance.Play(SoundType.BGM, SceneSettings.Instance[SceneAddress].BGM);
    }
}
