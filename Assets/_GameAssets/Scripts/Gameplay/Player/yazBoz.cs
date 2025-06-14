
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class NewMonoBehaviourScript : MonoBehaviour
{

    [Header("Referances")]
    [SerializeField] private Transform _transformMovement;
    private Rigidbody _rb;

    [Header("Movement Settings")]
    [SerializeField] private float _movementSpeed;
    [SerializeField] private KeyCode _movementKey;

    [Header("Jump Settings")]
    [SerializeField] private KeyCode _jumpKey;
    [SerializeField] private float _JumpForce;
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

    private float _verticalInput, _horizontalInput;

    private Vector3 _movementDirection;

    private bool _isSliding;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
    }

    private void Update()
    {
        SetInputs();
        SetPlayerDrag();
        LimitPlayerSpeed();
    }

    private void FixedUpdate()
    {
        SetPlayerMovement();
    }

    private void SetInputs()
    {
        _verticalInput = Input.GetAxisRaw("Vertical");
        _horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(_slideKey))
        {
            _isSliding = true;

        }
        else if (Input.GetKeyDown(_movementKey))
        {
            _isSliding = false;

        }

        else if (Input.GetKey(_jumpKey) && _canJump & IsGrounded())
        {
            _canJump = false;
            SetPlayerJumping();
            Invoke(nameof(ResetJumping), _jumpCoolDown);
        }


    }
    private void SetPlayerMovement()
    {
        _movementDirection = _transformMovement.forward * _verticalInput + _transformMovement.right * _horizontalInput;


        if (_isSliding)
        {
            _rb.AddForce(_movementDirection.normalized * _movementSpeed * _slideMultiplier, ForceMode.Force);

        }
        else
        {
            _rb.AddForce(_movementDirection.normalized * _movementSpeed, ForceMode.Force);

        }


    }


    private void SetPlayerDrag()
    {
        if (_isSliding)
        {
            _rb.linearDamping = _slideDrag;
        }
        else
        {
            _rb.linearDamping = _groundDrag;
        }


    }

    // anti hile sistemi
    private void LimitPlayerSpeed()
    {
        Vector3 flatVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);

        if (flatVelocity.magnitude > _movementSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * _movementSpeed;
            _rb.linearVelocity
                = new Vector3(limitedVelocity.x, _rb.linearVelocity.y, limitedVelocity.z);
        }
    }



    private void SetPlayerJumping()
    {
        // zıpladıktan sonra yukarı aşağı hareketi düzenleme için
        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);

        _rb.AddForce(transform.up * _JumpForce, ForceMode.Impulse);
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

}



