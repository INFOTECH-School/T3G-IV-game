using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Ustawienia Ruchu")]
    public float _acceleration = 10f; 
    public float _runningMultiplier = 1.67f;
    public float _drag = 1.5f;
    public float _jumpForce = 5f;

    [Header("Pchanie")]
    public float _pushSpeed = 3f;

    private Rigidbody _rigidBody;
    private Vector3 _inputDirection; // direction where player moves
    private bool _isGrounded;
    private bool _isJumpPressed;

    // Fixed rotation for isometric/angled camera
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
            // Reset velocity if game is paused
            _rigidBody.linearVelocity = new Vector3(0, _rigidBody.linearVelocity.y, 0);
            return;
        }

        // --- MERGE FIX HERE ---
        // logic: If pushing, do push logic. If not, do normal movement.
        if (_interactionScript && _interactionScript.currentState == Player.PlayerState.Pushing)
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

    // --- MERGE FIX: This is the correct version of the function ---
    private void HandleNormalInput()
    {
        // Calculate input relative to the camera angle (_rotation)
        var rawInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        _inputDirection = _rotation * rawInput;

        // Rotate character to face movement direction
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

        // If pushing, we handle movement manually (kinematic), so skip physics forces
        if (_interactionScript && _interactionScript.currentState == Player.PlayerState.Pushing)
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

        // Apply Drag (Friction)
        Vector3 friction = -horizontalVelocity * _drag;
        _rigidBody.AddForce(friction);
            
        // Apply Movement
        float speedMode = Input.GetKey(KeyCode.LeftShift) ? _runningMultiplier : 1f;
        _rigidBody.AddForce(_inputDirection * (_acceleration * speedMode));

        // Apply Jump
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
        
        // Prevent micro-movements
        if (Mathf.Abs(vertical) < 0.01f) return;

        Vector3 moveDir = transform.forward * vertical;
        // Move the PARENT object (the block), not just the player
        transform.parent.position += moveDir * (_pushSpeed * Time.deltaTime);
    }

    private void LateUpdate()
    {
        if (GameManager.Instance.CurrentGameState != GameManager.GameState.Gameplay) return;

        // Ensure player stays locked to the grab point while pushing
        if (!_interactionScript || _interactionScript.currentState != Player.PlayerState.Pushing)
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
        // Simple ground check
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
        if (GameManager.Instance) GameManager.Instance.UnregisterPlayerMovement();
    }
}