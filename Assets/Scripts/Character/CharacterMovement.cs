using UnityEngine;

#if UNITY_EDITOR   
using UnityEditor;
#endif

[RequireComponent(typeof(CharacterController))]
public class CharacterMovement : MonoBehaviour
{
    [field: SerializeField, ReadOnly]
    public bool IsGrounded { get; private set; } = true;

    [field: SerializeField, ReadOnly]
    public bool IsJumping { get; private set; }

    [field: SerializeField, ReadOnly]
    public bool IsFalling { get; private set; }

    [field: SerializeField, ReadOnly]
    public bool IsLanding { get; private set; }

    [field: Header("Move")]
    [field: SerializeField]
    public float MoveSpeed { get; set; }

    [field: SerializeField]
    public float SpeedChangeRate { get; set; }

    [field: Header("Rotation")]
    [field: SerializeField, Range(0f, 0.3f)]
    public float RotationSmoothTime { get; set; }

    [field: Header("Jump")]
    [field: SerializeField]
    public float JumpHeight { get; set; }

    [field: SerializeField]
    public float JumpTimeout { get; set; }

    [field: SerializeField]
    public float FallTimeout { get; set; }

    [field: SerializeField]
    public float LandTimeout { get; set; }

    [field: Header("Gravity")]
    [field: SerializeField]
    public bool UseGravity { get; set; } = true;

    [field: SerializeField, Min(1f)]
    public float GravityMultiplier { get; set; } = 1f;

    [field: Header("Grounded")]
    [field: SerializeField]
    public float GroundedOffset { get; set; } = -0.14f;

    [field: SerializeField]
    public float GroundedRadius { get; set; } = 0.28f;

    [field: SerializeField]
    public LayerMask GroundLayers { get; set; }

    private float _speed;
    private float _targetMove;
    private float _targetRotation;
    private Vector3 _targetDirection;
    private float _verticalVelocity;
    private float _rotationVelocity;
    private readonly float _gravity = -9.81f;
    private readonly float _terminalVelocity = 53f;

    // 점프관련 타임아웃
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;
    private float _landTimeoutDelta;
    private bool _isJumpingAndNotGrounding;
    private bool _canUpdateTimeouts;

    private CharacterController _controller;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        UpdateTimeouts();
        ApplyGravity();
        CheckGrounded();

        _controller.Move(_targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0f, _verticalVelocity * Time.deltaTime, 0f));
    }

    public void Move(Vector3 direction, float overrideYaw = 0f)
    {
        float targetSpeed = direction == Vector3.zero ? 0f : MoveSpeed;
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z).magnitude;
        float speedOffset = 0.1f;

        // 목표 속도까지 가감속
        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, SpeedChangeRate * Time.deltaTime);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        if (direction != Vector3.zero)
        {
            _targetMove = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + overrideYaw;
            _targetDirection = Quaternion.Euler(0f, _targetMove, 0f) * Vector3.forward;
        }
    }

    public void Rotate(Vector3 direction, float overrideYaw = 0f)
    {
        if (direction != Vector3.zero)
        {
            _targetRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + overrideYaw;
        }

        float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
        transform.rotation = Quaternion.Euler(0f, rotation, 0f);
    }

    public void Jump()
    {
        if (_jumpTimeoutDelta > 0f)
        {
            return;
        }

        IsJumping = true;
        IsFalling = false;
        IsLanding = false;
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
        _landTimeoutDelta = LandTimeout;
        _isJumpingAndNotGrounding = false;
        _canUpdateTimeouts = false;
        _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * _gravity * GravityMultiplier);
    }

    private void UpdateTimeouts()
    {
        if (IsGrounded)
        {
            if (_isJumpingAndNotGrounding)
            {
                IsJumping = false;
                IsFalling = false;
                IsLanding = true;
                _isJumpingAndNotGrounding = false;
                _canUpdateTimeouts = true;
            }

            if (_canUpdateTimeouts)
            {
                if (_jumpTimeoutDelta >= 0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }

                if (_landTimeoutDelta >= 0f)
                {
                    _landTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    IsLanding = false;
                }
            }
        }
        else
        {
            if (_fallTimeoutDelta >= 0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                IsJumping = false;
                IsFalling = true;
            }

            if (!_isJumpingAndNotGrounding)
            {
                _isJumpingAndNotGrounding = true;
            }
        }
    }

    private void ApplyGravity()
    {
        if (UseGravity)
        {
            if (IsGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }

            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += _gravity * GravityMultiplier * Time.deltaTime;
            }
        }
        else
        {
            _verticalVelocity = 0f;
        }
    }

    private void CheckGrounded()
    {
        var spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        IsGrounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
    }

    private void OnDrawGizmosSelected()
    {
        var transparentGreen = new Color(0f, 1f, 0f, 0.35f);
        var transparentRed = new Color(1f, 0f, 0f, 0.35f);

        if (IsGrounded)
        {
            Gizmos.color = transparentGreen;
        }
        else
        {
            Gizmos.color = transparentRed;
        }

        var spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Gizmos.DrawSphere(spherePosition, GroundedRadius);
#if UNITY_EDITOR
        Handles.Label(spherePosition, "Check Grounded");
#endif
    }
}
