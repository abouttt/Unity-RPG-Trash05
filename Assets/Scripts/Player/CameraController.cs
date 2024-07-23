using UnityEngine;

public class CameraController : MonoBehaviour
{
    [field: SerializeField]
    public Transform CinemachineCameraTarget { get; set; }

    [field: SerializeField]
    public float Sensitivity { get; set; }

    [field: SerializeField]
    public float TopClamp { get; set; }

    [field: SerializeField]
    public float BottomClamp { get; set; }

    private Quaternion _currentRotation;
    private float _cinemachineTargetPitch;
    private float _cinemachineTargetYaw;

    private void Start()
    {
        _cinemachineTargetPitch = CinemachineCameraTarget.rotation.eulerAngles.x;
        _cinemachineTargetYaw = CinemachineCameraTarget.rotation.eulerAngles.y;
    }

    public void Rotate(float pitch, float yaw)
    {
        _cinemachineTargetPitch += pitch * Sensitivity;
        _cinemachineTargetYaw += yaw * Sensitivity;
        ClampAngleAndRotate();
    }

    public void LookRotate(Vector3 direction, float speed)
    {
        var lookRotation = Quaternion.LookRotation(direction - CinemachineCameraTarget.position);
        var rotation = Quaternion.Slerp(_currentRotation, lookRotation, speed * Time.deltaTime);
        var euler = rotation.eulerAngles;
        _cinemachineTargetPitch = euler.x;
        _cinemachineTargetYaw = euler.y;
        ClampAngleAndRotate();
    }

    private void ClampAngleAndRotate()
    {
        _cinemachineTargetPitch = Util.ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
        _cinemachineTargetYaw = Util.ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _currentRotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0f);
        CinemachineCameraTarget.rotation = _currentRotation;
    }
}
