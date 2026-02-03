using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float _acceleration = 10f;
    public float _runningMultiplier = 1.67f;
    public float _drag = 1.5f;
    public float _jumpForce = 5f;
    public float _pushSpeed = 3f;

    [Header("References")]
    private Rigidbody _rigidBody;
    private Animator _animator;
    private PlayerInteraction _interactionScript;

    // State Variables
    private Vector3 _inputDirection;
    private bool _isGrounded;
    private bool _isJumpPressed;
    private readonly Quaternion _rotation = Quaternion.Euler(0, 45, 0); // Isometric input offset

    // Animator Hashes
    private int _speedHash;
    private int _groundedHash;
    private int _runningHash;

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _interactionScript = GetComponent<PlayerInteraction>();
        
        _animator = GetComponent<Animator>();
        if (_animator)
        {
            _speedHash = Animator.StringToHash("Speed");
            _groundedHash = Animator.StringToHash("isGrounded");
            _runningHash = Animator.StringToHash("isRunning");
        }

        if (GameManager.Instance) GameManager.Instance.RegisterPlayerMovement(this);
    }

    void Update()
    {
        // 1. Check Interaction State
        bool isPushing = _interactionScript && _interactionScript.currentState == Player.PlayerState.Pushing;

        if (isPushing)
        {
            // Handle Push/Pull logic (Kinematic movement)
            PushMovement();
        }
        else
        {
            // Handle Normal Walking logic (Input reading)
            HandleNormalInput();
        }

        // 2. Update Animations (works in both states)
        UpdateAnimator(isPushing);
    }

    private void FixedUpdate()
    {
        // Physics forces only apply when NOT pushing (Normal state)
        if (!_interactionScript || _interactionScript.currentState != Player.PlayerState.Pushing)
        {
            ApplyNormalPhysics();
        }
    }

    // Ensures player stays locked to the block frame-perfectly
    void LateUpdate()
    {
        if (_interactionScript && _interactionScript.currentState == Player.PlayerState.Pushing)
        {
            if (transform.parent)
            {
                PushableObject pushable = transform.parent.GetComponent<PushableObject>();
                if (pushable && pushable.grabPoint)
                {
                    transform.position = pushable.grabPoint.position;
                    transform.rotation = pushable.grabPoint.rotation;
                }
            }
        }
    }

    // --- MOVEMENT LOGIC ---

    private void HandleNormalInput()
    {
        // Calculate Direction based on Camera angle (45 deg)
        var rawInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        _inputDirection = _rotation * rawInput;

        // Rotate Character to face movement direction
        if (_inputDirection.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(_inputDirection);
        }

        // Jump Input
        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        {
            _isJumpPressed = true;
        }
    }

    private void ApplyNormalPhysics()
    {
        // 1. Friction (Drag)
        // Using linearVelocity (Unity 6+) or velocity (Older Unity)
        Vector3 currentVelocity = _rigidBody.linearVelocity; 
        Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
        
        Vector3 friction = -horizontalVelocity * _drag;
        _rigidBody.AddForce(friction);

        // 2. Movement Force
        float speedMode = Input.GetKey(KeyCode.LeftShift) ? _runningMultiplier : 1f;
        
        // Only add force if there is input
        _rigidBody.AddForce(_inputDirection * (_acceleration * speedMode));

        // 3. Jump Force
        if (_isJumpPressed)
        {
            _rigidBody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            _isJumpPressed = false;
            _isGrounded = false;
        }
    }

    private void PushMovement()
    {
        if (transform.parent)
        {
            // Only read Forward/Backward input (W/S)
            float verticalInput = Input.GetAxisRaw("Vertical");
            if (Mathf.Abs(verticalInput) < 0.01f) return;

            // Move in the direction the player is facing (Forward/Backward)
            Vector3 moveDirection = transform.forward * verticalInput;
            
            // Move the PARENT (the block)
            transform.parent.position += moveDirection * (_pushSpeed * Time.deltaTime);
        }
    }

    // --- ANIMATION & COLLISION ---

    private void UpdateAnimator(bool isPushing)
    {
        if (!_animator) return;

        if (isPushing)
        {
            // When pushing, we might want a specific "Push" bool in the future.
            // For now, we set speed to 0 so it doesn't play the run animation.
            _animator.SetFloat(_speedHash, 0); 
            _animator.SetBool(_groundedHash, true);
            _animator.SetBool(_runningHash, false);
        }
        else
        {
            // Normal Movement Animation
            Vector3 horizontalVelocity = new Vector3(_rigidBody.linearVelocity.x, 0, _rigidBody.linearVelocity.z);
            float currentSpeed = horizontalVelocity.magnitude;

            if (currentSpeed < 0.1f) currentSpeed = 0.0f;
            
            _animator.SetFloat(_speedHash, currentSpeed);
            _animator.SetBool(_groundedHash, _isGrounded);
            _animator.SetBool(_runningHash, Input.GetKey(KeyCode.LeftShift));
        }
    }

    void OnCollisionStay(Collision collision)
    {
        // Ground Check
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