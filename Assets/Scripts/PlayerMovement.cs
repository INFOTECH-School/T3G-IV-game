using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Ustawienia Ruchu (Movement Settings)")]
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
        _animator = GetComponent<Animator>();
        _interactionScript = GetComponent<PlayerInteraction>();
        
        if (_animator)
        {
            _speedHash = Animator.StringToHash("Speed");
            _groundedHash = Animator.StringToHash("isGrounded");
            _runningHash = Animator.StringToHash("isRunning");
        }

        if (GameManager.Instance) GameManager.Instance.RegisterPlayerMovement(this);
    }

    private void Update()
    {
        // GameManager Check
        if (GameManager.Instance != null && GameManager.Instance.CurrentGameState != GameManager.GameState.Gameplay)
        {
            _rigidBody.linearVelocity = new Vector3(0, _rigidBody.linearVelocity.y, 0);
            return;
        }

        // --- INTERACTION STATES ---
        if (_interactionScript && _interactionScript.currentState == Player.PlayerState.Pushing)
        {
            PushMovement();
        }
        else if (_interactionScript && _interactionScript.currentState == Player.PlayerState.Interacting)
        {
            KinematicMovement();
        }
        else
        {
            // Normal Movement
            HandleNormalInput();
            HandleJumpInput();
        }
        
        UpdateAnimator();
    }

    private void HandleNormalInput()
    {
        var rawInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
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
        if (GameManager.Instance != null && GameManager.Instance.CurrentGameState != GameManager.GameState.Gameplay)
            return;

        // Skip physics if interacting with an object
        if (_interactionScript && 
           (_interactionScript.currentState == Player.PlayerState.Pushing || 
            _interactionScript.currentState == Player.PlayerState.Interacting))
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
        
        // Horizontal friction allows jumping to feel natural without vertical drag
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
        Vector3 friction = -horizontalVelocity * _drag;
        _rigidBody.AddForce(friction);
            
        float speedMode = Input.GetKey(KeyCode.LeftShift) ? _runningMultiplier : 1f;
        
        _rigidBody.AddForce(_inputDirection * (_acceleration * speedMode));

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

        // Uses player's forward vector to dictate push direction (From Script 2)
        Vector3 moveDir = transform.forward * vertical;
        transform.parent.position += moveDir * (_pushSpeed * Time.deltaTime);
    }

    private void KinematicMovement()
    {
        if (!_interactionScript || !_interactionScript.CurrentKinematicTarget) return;

        KinematicObject kinematicObj = _interactionScript.CurrentKinematicTarget;
        float verticalInput = Input.GetAxisRaw("Vertical");
        
        // Only advance if W is pressed
        if (verticalInput > 0.01f)
        {
            kinematicObj.AdvanceMovement(Time.deltaTime);
            
            if (kinematicObj.HasReachedTarget())
            {
                if (kinematicObj.levelObjective)
                {
                    GameManager.Instance.LevelOperator.ProgressLevel();
                }

                Debug.Log("[PlayerMovement] Kinematic object reached target, auto-releasing.");
            }
        }
    }

    private void LateUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentGameState != GameManager.GameState.Gameplay) 
            return;

        if (!_interactionScript || !transform.parent) return;

        // Snap player securely to the object's grab points to prevent sliding
        if (_interactionScript.currentState == Player.PlayerState.Pushing)
        {
            PushableObject pushable = transform.parent.GetComponent<PushableObject>();
            if (pushable && pushable.grabPoint)
            {
                transform.position = pushable.grabPoint.position;
                transform.rotation = pushable.grabPoint.rotation;
            }
        }
        else if (_interactionScript.currentState == Player.PlayerState.Interacting)
        {
            KinematicObject kinematic = transform.parent.GetComponent<KinematicObject>();
            if (kinematic && kinematic.grabPosition)
            {
                transform.position = kinematic.grabPosition.position;
                transform.rotation = kinematic.grabPosition.rotation;
            }
        }
    }

    // --- ANIMATION & PHYSICS COLLISIONS ---

    private void UpdateAnimator()
    {
        if (!_animator) return;

        Vector3 horizontalVel = new Vector3(_rigidBody.linearVelocity.x, 0, _rigidBody.linearVelocity.z);
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
        if (GameManager.Instance) GameManager.Instance.UnregisterPlayerMovement();
    }
}