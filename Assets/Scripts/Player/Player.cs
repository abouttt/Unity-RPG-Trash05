using UnityEngine;
using UnityEngine.InputSystem;

public class Player : SingletonBehaviour<Player>
{
    private Vector2 _move;

    private Camera _mainCamera;
    private CharacterMovement _movement;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _movement = GetComponent<CharacterMovement>();
    }

    private void Update()
    {
        MoveAndRotate();
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

    private void OnJump()
    {
        _movement.Jump();
    }
    #endregion
}
