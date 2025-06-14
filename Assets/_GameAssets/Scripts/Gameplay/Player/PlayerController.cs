using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _orientationTransform;


    [Header("Movement Settings")]
    [SerializeField] private float _movementSpeed;

    [SerializeField] private KeyCode _movementKey;


    [Header("Jump Settings")]
    [SerializeField] private KeyCode _jumpKey;
    [SerializeField] private float _jumpForce;

    [SerializeField] private float _airMultiplier;

    [SerializeField] private float _airDrag;
    [SerializeField] private bool _canJump;
    [SerializeField] private float _jumpCoolDown;


    [Header("Sliding Settings")]
    [SerializeField] private KeyCode _slideKey;
    // slide hızı
    [SerializeField] private float _slideMultiplier;
    [SerializeField] private float _slideDrag;

    [Header("Ground Check Settings")]

    [SerializeField] private float _playerHeight;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundDrag;

    private StateController _stateController;
    private Rigidbody _playerRigidBody;

    private float _horizontalInput, _verticalInput;

    private Vector3 _movementDirection;

    private bool _isSliding;


    private void Awake()
    {
        _stateController = GetComponent<StateController>();
        _playerRigidBody = GetComponent<Rigidbody>();
        _playerRigidBody.freezeRotation = true;
    }

    private void Update()
    {
        SetInputs();
        SetStates();
        SetPlayerDrag();
        LimitPlayerSpeed();

    }

    private void FixedUpdate()
    {
        SetPlayerMovement();

    }

    private void SetInputs()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(_slideKey))
        {
            _isSliding = true;

        }
        else if (Input.GetKeyDown(_movementKey))
        {
            _isSliding = false;

        }


        if (Input.GetKey(_jumpKey) && _canJump & IsGrounded())
        {
            _canJump = false;
            // Zıplama İşlemi Gerçekleşecek!
            SetPlayerJumping();
            Invoke(nameof(ResetJumping), _jumpCoolDown);
        }
    }


    // İşleri kolaylaştırmak
    private void SetStates()
    {
        var movementDirection = GetMovementDircetion();
        var isGrounded = IsGrounded();
        var isSliding = IsSliding();
        var currentState = _stateController.GetCurrentState();

        var newState = currentState switch
        {
            _ when movementDirection == Vector3.zero && isGrounded && !_isSliding => PlayerState.Idle,
            _ when movementDirection != Vector3.zero && isGrounded && !_isSliding => PlayerState.Move,
            _ when movementDirection != Vector3.zero && isGrounded && _isSliding => PlayerState.Slide,
            _ when movementDirection == Vector3.zero && isGrounded && _isSliding => PlayerState.SlideIdle,
            _ when !_canJump && !isGrounded => PlayerState.Jump,
            _ => currentState
        };


        if (newState != currentState)
        {
            _stateController.ChangeState(newState);
        }
    }

    private void SetPlayerMovement()
    {
        _movementDirection = _orientationTransform.forward * _verticalInput + _orientationTransform.right * _horizontalInput;

        float forceMultiplier = _stateController.GetCurrentState() switch
        {
            PlayerState.Move => 1f,
            PlayerState.Slide => _slideMultiplier,
            PlayerState.Jump => _airMultiplier,
            _ => 1f
        };

        _playerRigidBody.AddForce(_movementDirection.normalized * _movementSpeed * forceMultiplier, ForceMode.Force);

    }


    private void SetPlayerDrag()
    {
        _playerRigidBody.linearDamping = _stateController.GetCurrentState() switch
        {
            PlayerState.Move => _groundDrag,
            PlayerState.Slide => _slideDrag,
            PlayerState.Jump => _airDrag,
            _ => _playerRigidBody.linearDamping
        };

    }

    // anti hile sistemi
    private void LimitPlayerSpeed()
    {
        Vector3 flatVelocity = new Vector3(_playerRigidBody.linearVelocity.x, 0f, _playerRigidBody.linearVelocity.z);

        if (flatVelocity.magnitude > _movementSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * _movementSpeed;
            _playerRigidBody.linearVelocity
                = new Vector3(limitedVelocity.x, _playerRigidBody.linearVelocity.y, limitedVelocity.z);
        }
    }



    private void SetPlayerJumping()
    {
        // zıpladıktan sonra yukarı aşağı hareketi düzenleme için
        _playerRigidBody.linearVelocity = new Vector3(_playerRigidBody.linearVelocity.x, 0f, _playerRigidBody.linearVelocity.z);

        _playerRigidBody.AddForce(transform.up * _jumpForce, ForceMode.Impulse);
    }


    // zıplamanın kilidini kaldırıyor mesela 0.3sn sonra bu kilidi açıyoruz
    private void ResetJumping()
    {
        _canJump = true;
    }

    // karakterin yerde olup olmadığını tespit ediyor ve havada zıplamaya izin vermiyoruz
    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, _playerHeight * 0.5f + 0.2f, _groundLayer);
    }

    private Vector3 GetMovementDircetion()
    {
        return _movementDirection.normalized;
    }

    private bool IsSliding()
    {
        return _isSliding;
    }
}
