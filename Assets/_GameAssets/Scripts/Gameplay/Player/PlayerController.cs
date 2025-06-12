using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _orientationTransform;


    [Header("Movement Settings")]
    [SerializeField] private float _movementSpeed;


    [Header("Jump Settings")]
    [SerializeField] private KeyCode _jumpKey;
    [SerializeField] private float _jumpForce;
    [SerializeField] private bool _canJump;
    [SerializeField] private float _jumpCoolDown;

    [Header("Ground Check Settings")]

    [SerializeField] private float _playerHeight;
    [SerializeField] private LayerMask _groundLayer;
    private Rigidbody _playerRigidBody;

    private float _horizontalInput, _verticalInput;

    private Vector3 _movementDirection;

    private void Awake()
    {
        _playerRigidBody = GetComponent<Rigidbody>();
        _playerRigidBody.freezeRotation = true;
    }

    private void Update()
    {
        SetInputs();
    }

    private void FixedUpdate()
    {
        SetPlayerMovement();

    }

    private void SetInputs()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(_jumpKey) && _canJump & IsGrounded())
        {
            _canJump = false;
            // Zıplama İşlemi Gerçekleşecek!
            SetPlayerJumping();
            Invoke(nameof(ResetJumping), _jumpCoolDown);
        }
    }

    private void SetPlayerMovement()
    {
        _movementDirection = _orientationTransform.forward * _verticalInput + _orientationTransform.right * _horizontalInput;

        _playerRigidBody.AddForce(_movementDirection.normalized * _movementSpeed, ForceMode.Force);
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
}
