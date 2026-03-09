using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float _acceleration = 10f;
    public float _runningMultiplier = 1.67f;
    public float _drag = 1.5f;
    public float _jumpForce = 5f;
    public float _pushSpeed = 3f;
    [Tooltip("How quickly the player stops when grounded and not moving.")]
    public float stoppingSpeed = 5f;

    [Header("Throw Settings")]
    public GameObject _itemPrefab;
    public Transform _throwingPoint;
    public LineRenderer _lineRenderer;
    public LayerMask _groundLayer;
    public int _lineSegments = 30;
    public float _timeBetweenDots = 0.1f;
    public float _timeToTarget = 1f;
    public bool throwingEnabled = true;

    [Header("References")]
    private Rigidbody _rigidBody;
    private Animator _animator;
    private PlayerInteraction _interactionScript;

    // State Variables
    private Vector3 _inputDirection;
    private bool _isGrounded;
    private bool _isJumpPressed;
    private bool _isAiming;
    private bool _hasValidTarget;
    private Vector3 _calculatedVelocity;
    private readonly Quaternion _rotation = Quaternion.Euler(0, 45, 0); // Isometric offset

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
        // 1. Safety Check
        if (GameManager.Instance != null && GameManager.Instance.CurrentGameState != GameManager.GameState.Gameplay)
        {
            _rigidBody.linearVelocity = new Vector3(0, _rigidBody.linearVelocity.y, 0);
            return;
        }

        // 2. Handle State Logic
        bool isPushing = _interactionScript && _interactionScript.currentState == Player.PlayerState.Pushing;
        bool isInteracting = _interactionScript && _interactionScript.currentState == Player.PlayerState.Interacting;

        if (!isInteracting && !isPushing)
        {
            HandleNormalInput();
            HandleAimingInput();
            HandleJumpInput();
        }

        UpdateAnimator();
    }

    private void HandleNormalInput()
    {
        var rawInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        _inputDirection = _rotation * rawInput;

        // Only rotate toward movement if we aren't aiming
        if (!_isAiming && _inputDirection.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(_inputDirection);
        }
    }

    private void HandleAimingInput()
    {
        if (!throwingEnabled) return;
        // Start Aiming (Only if grounded and not moving fast)
        Vector3 horizontalVel = new Vector3(_rigidBody.linearVelocity.x, 0, _rigidBody.linearVelocity.z);
        if (Input.GetMouseButtonDown(1) && _isGrounded && horizontalVel.magnitude < 0.5f)
        {
            _isAiming = true;
        }

        if (_isAiming)
        {
            if (Input.GetMouseButton(1))
            {
                Aim();
            }
            
            if (Input.GetMouseButtonUp(1))
            {
                _isAiming = false;
                HideTrajectory();
                if (_hasValidTarget) ThrowItem();
            }
        }
    }

    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded && !_isAiming)
            _isJumpPressed = true;
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentGameState != GameManager.GameState.Gameplay)
            return;

        bool isPushing = _interactionScript && _interactionScript.currentState == Player.PlayerState.Pushing;
        bool isInteracting = _interactionScript && _interactionScript.currentState == Player.PlayerState.Interacting;
    
        _rigidBody.isKinematic = isPushing || isInteracting;

        if (isInteracting)
        {
            KinematicMovement();
        }
        else if (isPushing)
        {
            PushMovement();
        }
        else
        {
            ApplyNormalPhysics();
        }
    }

    private void ApplyNormalPhysics()
    {
        // 1. Braking/Friction
        if (_isGrounded && _inputDirection.magnitude < 0.1f)
        {
            // Snappy stop when no input
            var targetVel = new Vector3(0, _rigidBody.linearVelocity.y, 0);
            _rigidBody.linearVelocity = Vector3.Lerp(_rigidBody.linearVelocity, targetVel, stoppingSpeed * Time.fixedDeltaTime);
        }
        else
        {
            // Standard air/move friction
            Vector3 horizontalVelocity = new Vector3(_rigidBody.linearVelocity.x, 0, _rigidBody.linearVelocity.z);
            _rigidBody.AddForce(-horizontalVelocity * _drag);
        }

        // 2. Movement (Disabled while aiming)
        if (!_isAiming)
        {
            float speedMode = Input.GetKey(KeyCode.LeftShift) ? _runningMultiplier : 1f;
            _rigidBody.AddForce(_inputDirection * (_acceleration * speedMode));
        }

        // 3. Jump
        if (_isJumpPressed)
        {
            _rigidBody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            _isJumpPressed = false;
            _isGrounded = false;
        }
    }

    #region Throwing Logic
    private void Aim()
    {
        if (!Camera.current.CompareTag("MainCamera")) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, _groundLayer))
        {
            _hasValidTarget = true;

            // Rotate player to face target
            Vector3 lookDirection = hit.point - transform.position;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero) transform.rotation = Quaternion.LookRotation(lookDirection);

            _calculatedVelocity = CalculateVelocity(_throwingPoint.position, hit.point, _timeToTarget);
            DrawTrajectory(_calculatedVelocity);
        }
        else
        {
            _hasValidTarget = false;
            HideTrajectory();
        }
    }

    private Vector3 CalculateVelocity(Vector3 start, Vector3 target, float time)
    {
        Vector3 distanceXZ = new Vector3(target.x - start.x, 0, target.z - start.z);
        float distanceY = target.y - start.y;

        Vector3 velocityXZ = distanceXZ / time;
        float velocityY = (distanceY - 0.5f * Physics.gravity.y * time * time) / time;
        
        return new Vector3(velocityXZ.x, velocityY, velocityXZ.z);
    }

    private void DrawTrajectory(Vector3 velocity)
    {
        _lineRenderer.positionCount = _lineSegments;
        Vector3[] positions = new Vector3[_lineSegments];
        for (int i = 0; i < _lineSegments; i++)
        {
            float t = i * _timeBetweenDots;
            positions[i] = _throwingPoint.position + (velocity * t) + (Physics.gravity * (0.5f * t * t));
        }
        _lineRenderer.SetPositions(positions);
    }

    private void HideTrajectory() => _lineRenderer.positionCount = 0;

    private void ThrowItem()
    {
        GameObject projectile = Instantiate(_itemPrefab, _throwingPoint.position, Quaternion.identity);
        if (projectile.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = _calculatedVelocity;
        }
    }
    #endregion

    #region Interaction Logic
    private void PushMovement()
    {
        if (!transform.parent) return;
        float vertical = Input.GetAxisRaw("Vertical");
        if (Mathf.Abs(vertical) < 0.01f) return;

        Vector3 moveDir = transform.forward * vertical;
        transform.parent.position += moveDir * (_pushSpeed * Time.fixedDeltaTime);
    }

    private void KinematicMovement()
    {
        if (!_interactionScript || !_interactionScript.CurrentKinematicTarget) return;

        KinematicObject kinematicObj = _interactionScript.CurrentKinematicTarget;
        if (Input.GetAxisRaw("Vertical") > 0.01f)
        {
            kinematicObj.AdvanceMovement(Time.fixedDeltaTime);
            if (kinematicObj.HasReachedTarget())
            {
                _interactionScript.ToggleKinematicMode();
                if (kinematicObj.movementType != KinematicObject.MovementType.Car)
                {
                    kinematicObj.targetTransform.gameObject.SetActive(false);
                    kinematicObj.CompleteObjective();
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (!_interactionScript || !transform.parent) return;

        // Snap to grab points
        if (_interactionScript.currentState == Player.PlayerState.Pushing)
        {
            var pushable = transform.parent.GetComponent<PushableObject>();
            if (pushable && pushable.grabPoint)
            {
                transform.position = pushable.grabPoint.position;
                transform.rotation = pushable.grabPoint.rotation;
            }
        }
        else if (_interactionScript.currentState == Player.PlayerState.Interacting)
        {
            var kinematic = transform.parent.GetComponent<KinematicObject>();
            if (kinematic && kinematic.grabPosition)
            {
                transform.position = kinematic.grabPosition.position;
                transform.rotation = kinematic.grabPosition.rotation;
            }
        }
    }
    #endregion

    private void UpdateAnimator()
    {
        if (!_animator) return;
        Vector3 horizontalVel = new Vector3(_rigidBody.linearVelocity.x, 0, _rigidBody.linearVelocity.z);
        float speed = horizontalVel.magnitude;
        
        _animator.SetFloat(_speedHash, speed < 0.1f ? 0f : speed);
        _animator.SetBool(_groundedHash, _isGrounded);
        _animator.SetBool(_runningHash, Input.GetKey(KeyCode.LeftShift));
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (var contact in collision.contacts)
        {
            if (contact.normal.y > 0.7f) { _isGrounded = true; return; }
        }
    }

    private void OnCollisionExit() => _isGrounded = false;

    private void OnDestroy()
    {
        if (GameManager.Instance) GameManager.Instance.UnregisterPlayerMovement();
    }
}