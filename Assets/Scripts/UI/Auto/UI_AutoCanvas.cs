using UnityEngine;

public class UI_AutoCanvas : UI_Base
{
    protected override void Init()
    {
        UIManager.Instance.Register(this);
    }
}
