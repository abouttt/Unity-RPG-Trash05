using UnityEngine;

public abstract class UI_Auto : UI_Base
{
    [SerializeField]
    protected GameObject Body;

    protected virtual void Start()
    {
        if (Body == null)
        {
            Body = transform.GetChild(0).gameObject;
        }

        Body.SetActive(false);
        transform.SetParent(UIManager.Instance.Get<UI_AutoCanvas>().transform);
    }
}
