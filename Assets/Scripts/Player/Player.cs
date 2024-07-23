using UnityEngine;
using UnityEngine.InputSystem;

public class Player : SingletonBehaviour<Player>
{
    private Vector2 _move;
    private Vector2 _look;

    private Camera _mainCamera;
    private CharacterMovement _movement;
    private CameraController _cameraController;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _movement = GetComponent<CharacterMovement>();
        _cameraController = GetComponent<CameraController>();
    }

    private void Update()
    {
        MoveAndRotate();
    }

    private void LateUpdate()
    {
        _cameraController.Rotate(_look.y, _look.x);
    }

    private void MoveAndRotate()
    {
        var inputDirection = new Vector3(_move.x, 0f, _move.y);
        float cameraYaw = _mainCamera.transform.eulerAngles.y;

        _movement.Move(inputDirection, cameraYaw);
        _movement.Rotate(inputDirection, cameraYaw);
    }

    #region Input
    private void OnMove(InputValue inputValue)
    {
        _move = inputValue.Get<Vector2>();
    }

    private void OnLook(InputValue inputValue)
    {
        if (InputManager.Instance.CursorLocked)
        {
            _look = inputValue.Get<Vector2>();
        }
        else
        {
            _look = Vector2.zero;
        }
    }

    private void OnJump()
    {
        _movement.Jump();
    }
    #endregion
}
