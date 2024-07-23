using UnityEngine;
using UnityEngine.InputSystem;

public class Player : SingletonBehaviour<Player>
{
    public LockOn LockOn => _lockOn;

    [SerializeField]
    private float _runSpeed;

    [SerializeField]
    private float _sprintSpeed;

    [SerializeField]
    private float _landingSpeed;

    [SerializeField]
    private float _lockOnRotationSpeed;

    private Camera _mainCamera;
    private CharacterMovement _movement;
    private CameraController _cameraController;
    private LockOn _lockOn;

    // Input
    private Vector2 _move;
    private Vector2 _look;
    private bool _isPressedSprint;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _movement = GetComponent<CharacterMovement>();
        _cameraController = GetComponent<CameraController>();
        _lockOn = GetComponent<LockOn>();
    }

    private void Update()
    {
        CharacterMoveAndRotate();
    }

    private void LateUpdate()
    {
        CameraRotate();
        _lockOn.TrackingTarget(_mainCamera.transform);
    }

    private void CharacterMoveAndRotate()
    {
        var inputDirection = new Vector3(_move.x, 0f, _move.y);
        float cameraYaw = _mainCamera.transform.eulerAngles.y;

        _movement.MoveSpeed = _movement.IsLanding ? _landingSpeed
                            : _isPressedSprint ? _sprintSpeed
                            : _runSpeed;
        _movement.Move(inputDirection, cameraYaw);

        if (_lockOn.IsLockOn && IsOnlyRun())
        {
            var rotationDirection = inputDirection == Vector3.zero
                                    ? Vector3.zero
                                    : (_lockOn.Target.position - transform.position).normalized;
            _movement.Rotate(rotationDirection);
        }
        else
        {
            _movement.Rotate(inputDirection, cameraYaw);
        }
    }

    private void CameraRotate()
    {
        if (_lockOn.IsLockOn)
        {
            var direction = (_lockOn.Target.position + transform.position) * 0.5f;
            _cameraController.LookRotate(direction, _lockOnRotationSpeed);
        }
        else
        {
            _cameraController.Rotate(_look.y, _look.x);
        }
    }

    private bool IsOnlyRun()
    {
        return !(_isPressedSprint || _movement.IsJumping || _movement.IsFalling || _movement.IsLanding);
    }

    #region Input
    private void OnMove(InputValue inputValue)
    {
        _move = inputValue.Get<Vector2>();
    }

    private void OnLook(InputValue inputValue)
    {
        _look = InputManager.Instance.CursorLocked ? inputValue.Get<Vector2>() : Vector2.zero;
    }

    private void OnJump()
    {
        _movement.Jump();
    }

    public void OnSprint(InputValue inputValue)
    {
        _isPressedSprint = inputValue.isPressed;
    }

    private void OnLockOn()
    {
        if (_lockOn.IsLockOn)
        {
            _lockOn.Target = null;
        }
        else
        {
            if (_lockOn.FindTarget(_mainCamera.transform))
            {
                var planes = GeometryUtility.CalculateFrustumPlanes(_mainCamera);
                var bounds = _lockOn.Target.GetComponent<Collider>().bounds;
                if (!GeometryUtility.TestPlanesAABB(planes, bounds))
                {
                    _lockOn.Target = null;
                }
            }
        }
    }
    #endregion
}
