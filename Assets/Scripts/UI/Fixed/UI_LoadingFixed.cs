using UnityEngine;

public class UI_LoadingFixed : UI_Base
{
    enum Images
    {
        BG,
        Bar,
    }

    protected override void Init()
    {
        BindImage(typeof(Images));
    }

    private void Start()
    {
        var bg = GetImage((int)Images.BG);
        bg.sprite = SceneSettings.Instance[SceneLoader.Instance.NextSceneAddress].Background;
        bg.color = Color.white;
        if (bg.sprite == null)
        {
            bg.color = Color.black;
        }

        GetImage((int)Images.Bar).fillAmount = 0f;
    }

    private void Update()
    {
        GetImage((int)Images.Bar).fillAmount = SceneLoader.Instance.LoadingProgress;
    }
}
