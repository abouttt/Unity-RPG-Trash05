using UnityEngine;

[RequireComponent(typeof(UI_FollowWorldObject))]
public class UI_LockOn : UI_Auto
{
    private UI_FollowWorldObject _followTarget;

    protected override void Init()
    {
        _followTarget = GetComponent<UI_FollowWorldObject>();
    }

    protected override void Start()
    {
        base.Start();

        Player.Instance.LockOn.TargetChanged += target =>
        {
            Body.SetActive(target != null);
            _followTarget.Target = target;
        };
    }
}
