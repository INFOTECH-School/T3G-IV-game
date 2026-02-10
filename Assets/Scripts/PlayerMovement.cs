using System;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    [Header("Ruch")]
    public float _acceleration = 10f;
    public float _runningMultiplier = 1.67f;
    public float _drag = 1.5f;
    public float _jumpForce = 5f;

    [Header("Pchanie")]
    public float _pushSpeed = 3f;

    private Rigidbody _rigidBody;
    private Vector3 _inputDirection; // direction where player moves
    private Vector3 _currentVelocity; 
    private bool _isGrounded;
    private bool _isJumpPressed;

    private readonly Quaternion _rotation = Quaternion.Euler(0, 45, 0);

    private Animator _animator;
    private int _speedHash;
    private int _groundedHash;
    private int _runningHash;

    private PlayerInteraction _interactionScript;

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _interactionScript = GetComponent<PlayerInteraction>();

        if (_animator)
        {
            _speedHash = Animator.StringToHash("Speed");
            _groundedHash = Animator.StringToHash("isGrounded");
            _runningHash = Animator.StringToHash("isRunning");
        }

        if (GameManager.Instance)
            GameManager.Instance.RegisterPlayerMovement(this);
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentGameState != GameManager.GameState.Gameplay)
        {
            _rigidBody.linearVelocity = new Vector3(0, _rigidBody.linearVelocity.y, 0);
            return;
        }

        if (_interactionScript &&
            _interactionScript.currentState == Player.PlayerState.Pushing)
        {
            PushMovement();
        }
        else
        {
            HandleNormalInput();
            HandleJumpInput();
        }

        UpdateAnimator();
    }

    private void HandleNormalInput()
    {
        Vector3 rawInput = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0,
            Input.GetAxisRaw("Vertical")
        );

        _inputDirection = _rotation * rawInput;

        if (_inputDirection.magnitude > 0.1f)
            transform.rotation = Quaternion.LookRotation(_inputDirection);
    }

    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
            _isJumpPressed = true;
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.CurrentGameState != GameManager.GameState.Gameplay)
        {
            _rigidBody.linearVelocity = new Vector3(0, _rigidBody.linearVelocity.y, 0);
            return;
        }
        if (_interactionScript &&
            _interactionScript.currentState == Player.PlayerState.Pushing)
        {
            _rigidBody.isKinematic = true;
            return;
        }

        _rigidBody.isKinematic = false;
        ApplyNormalPhysics();
    }

    private void ApplyNormalPhysics()
    {
        Vector3 velocity = _rigidBody.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);

        // tarcie
        Vector3 friction = -horizontalVelocity * _drag;
        _rigidBody.AddForce(friction);

        // ruch
        float speedMode = Input.GetKey(KeyCode.LeftShift) ? _runningMultiplier : 1f;
        _rigidBody.AddForce(_inputDirection * (_acceleration * speedMode));

        // skok
        if (_isJumpPressed)
        {
            _rigidBody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            _isJumpPressed = false;
            _isGrounded = false;
        }
    }

    private void PushMovement()
    {
        if (!transform.parent) return;

        float vertical = Input.GetAxisRaw("Vertical");
        if (Mathf.Abs(vertical) < 0.01f) return;

        Vector3 moveDir = transform.forward * vertical;
        transform.parent.position += moveDir * (_pushSpeed * Time.deltaTime);
    }

    private void LateUpdate()
    {
        if (GameManager.Instance.CurrentGameState != GameManager.GameState.Gameplay)
        {
            _rigidBody.linearVelocity = new Vector3(0, _rigidBody.linearVelocity.y, 0);
            return;
        }
        if (!_interactionScript ||
            _interactionScript.currentState != Player.PlayerState.Pushing)
            return;

        if (!transform.parent) return;

        PushableObject pushable = transform.parent.GetComponent<PushableObject>();
        if (pushable && pushable.grabPoint)
        {
            transform.position = pushable.grabPoint.position;
            transform.rotation = pushable.grabPoint.rotation;
        }
    }

    private void UpdateAnimator()
    {
        if (!_animator) return;

        Vector3 horizontalVel = new Vector3(
            _rigidBody.linearVelocity.x,
            0,
            _rigidBody.linearVelocity.z
        );

        float speed = horizontalVel.magnitude;
        if (speed < 0.1f) speed = 0f;

        _animator.SetFloat(_speedHash, speed);
        _animator.SetBool(_groundedHash, _isGrounded);
        _animator.SetBool(_runningHash, Input.GetKey(KeyCode.LeftShift));
    }

    private void OnCollisionStay(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y > 0.7f)
            {
                _isGrounded = true;
                return;
            }
        }
    }

    private void OnCollisionExit()
    {
        _isGrounded = false;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance)
            GameManager.Instance.UnregisterPlayerMovement();
    }
}
