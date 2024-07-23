using System;
using UnityEngine;

public class LockOn : MonoBehaviour
{
    public event Action<Transform> TargetChanged;

    public Transform Target
    {
        get => _target;
        set
        {
            _target = value;
            IsLockOn = _target != null;
            TargetChanged?.Invoke(_target);
        }
    }

    public bool IsLockOn { get; private set; }

    [field: SerializeField]
    public float ViewRadius { get; set; }

    [field: SerializeField]
    public float MinViewAngle { get; set; }

    [field: SerializeField]
    public float MaxViewAngle { get; set; }

    [field: SerializeField]
    public LayerMask TargetLayers { get; set; }

    [field: SerializeField]
    public LayerMask ObstacleLayers { get; set; }

    private Transform _target;

    public bool FindTarget(Transform start)
    {
        float shortestAngle = Mathf.Infinity;
        Transform finalTarget = null;

        var targets = Physics.OverlapSphere(start.position, ViewRadius, TargetLayers);
        foreach (var target in targets)
        {
            var directionToTarget = (target.transform.position - start.position).normalized;
            float viewAngle = Vector3.Angle(start.forward, directionToTarget);

            if (viewAngle < MinViewAngle ||
                viewAngle > MaxViewAngle ||
                viewAngle >= shortestAngle)
            {
                continue;
            }

            if (Physics.Linecast(start.position, target.transform.position, ObstacleLayers))
            {
                continue;
            }

            finalTarget = target.transform;
            shortestAngle = viewAngle;
        }

        Target = finalTarget;

        return IsLockOn;
    }

    public void TrackingTarget(Transform start, bool distance = true, bool obstacle = true, bool angle = true)
    {
        if (!IsLockOn)
        {
            return;
        }

        if (!_target.gameObject.activeInHierarchy)
        {
            Target = null;
            return;
        }

        if (distance && Vector3.Distance(start.position, _target.position) > ViewRadius)
        {
            Target = null;
            return;
        }

        if (obstacle && Physics.Linecast(start.position, _target.position, ObstacleLayers))
        {
            Target = null;
            return;
        }

        if (angle && !IsAngleInRange(start.eulerAngles.x))
        {
            Target = null;
        }
    }

    private bool IsAngleInRange(float angle)
    {
        angle = Util.ClampAngle(angle, MinViewAngle, MaxViewAngle);
        return angle >= MinViewAngle && angle <= MaxViewAngle;
    }
}
