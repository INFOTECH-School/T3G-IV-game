using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Animator), typeof(AudioSource))]
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
    public Transform _throwingPoint;
    public LineRenderer _lineRenderer;
    public LayerMask _groundLayer;
    public int _lineSegments = 30;
    public float _timeBetweenDots = 0.1f;
    [Tooltip("Controls the power of the throw. A higher value means a weaker, higher-arcing throw.")]
    public float _timeToTarget = 1.5f;
    public bool throwingEnabled = true;

    [Header("Audio Settings")]
    public AudioClip[] _footstepClips;
    public AudioClip _jumpClip;
    public float _walkStepRate = 0.5f;
    public float _runStepRate = 0.3f;
    private float _footstepTimer;


    [Header("References")]
    private Rigidbody _rigidBody;
    private Animator _animator;
    private PlayerInteraction _interactionScript;
    private AudioSource _audioSource;

    // State Variables
    private Vector3 _inputDirection;
    private bool _isGrounded;
    private bool _isJumpPressed;
    private bool _isAiming;
    private bool _hasValidTarget;
    private Vector3 _calculatedVelocity;
    private readonly Quaternion _rotation = Quaternion.Euler(0, 45, 0);
    private bool _isInValidThrowZone;
    public bool IsInValidThrowZone { get => _isInValidThrowZone; }


    // Animator Hashes
    private int _speedHash;
    private int _groundedHash;
    private int _runningHash;

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _interactionScript = GetComponent<PlayerInteraction>();
        _audioSource = GetComponent<AudioSource>();

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
        if (GameManager.Instance != null && GameManager.Instance.CurrentGameState != GameManager.GameState.Gameplay)
        {
            _rigidBody.linearVelocity = new Vector3(0, _rigidBody.linearVelocity.y, 0);
            return;
        }

        bool isPushing = _interactionScript && _interactionScript.currentState == Player.PlayerState.Pushing;
        bool isInteracting = _interactionScript && _interactionScript.currentState == Player.PlayerState.Interacting;

        if (!isInteracting && !isPushing)
        {
            HandleNormalInput();
            HandleAimingInput();
            HandleJumpInput();
            HandleFootstepSounds();
        }
        else if (isPushing)
        {
            // Pushing action happens natively here, but the tutorial hook is handled inside TutorialManager's Update loop to guarantee state precision.
        }

        UpdateAnimator();
    }

    private void HandleFootstepSounds()
    {
        if (!_isGrounded || _inputDirection.magnitude < 0.1f) return;

        _footstepTimer -= Time.deltaTime;
        if (_footstepTimer <= 0)
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            _footstepTimer = isRunning ? _runStepRate : _walkStepRate;

            if (_footstepClips != null && _footstepClips.Length > 0)
            {
                int index = Random.Range(0, _footstepClips.Length);
                AudioClip clip = _footstepClips[index];
                if (clip)
                {
                    _audioSource.PlayOneShot(clip);
                }
            }
        }
    }


    private void HandleNormalInput()
    {
        var rawInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        _inputDirection = _rotation * rawInput;

        if (rawInput.magnitude > 0.1f) _rigidBody.WakeUp();
        
        if (!_isAiming && _inputDirection.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(_inputDirection);
        }
    }

    private void HandleAimingInput()
    {
        if (!throwingEnabled || GameManager.Instance.Player == null) return;

        Vector3 horizontalVel = new Vector3(_rigidBody.linearVelocity.x, 0, _rigidBody.linearVelocity.z);
        if (Input.GetMouseButtonDown(1) && _isGrounded && horizontalVel.magnitude < 0.5f &&
            GameManager.Instance.Player.currentItem)
        {
            if (GameManager.Instance.Player.currentItem.CompareTag("Throwable"))
            {
                _isAiming = true;
            }
            else
            {
                var feedback = GameManager.Instance.Player.currentItem.GetComponentInChildren<InteractionFeedback>();
                if (feedback) feedback.ShowErrorFeedback();
            }
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _rigidBody.WakeUp();

            if (_isGrounded && !_isAiming)
            {
                _isJumpPressed = true;
                if (_jumpClip)
                {
                    _audioSource.PlayOneShot(_jumpClip);
                }
                if (TutorialManager.Instance != null)
                {
                    TutorialManager.Instance.OnJump();
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentGameState != GameManager.GameState.Gameplay)
            return;

        _isGrounded = false;

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
        if (_isGrounded && _inputDirection.magnitude < 0.1f)
        {
            var targetVel = new Vector3(0, _rigidBody.linearVelocity.y, 0);
            _rigidBody.linearVelocity = Vector3.Lerp(_rigidBody.linearVelocity, targetVel, stoppingSpeed * Time.fixedDeltaTime);
        }
        else
        {
            Vector3 horizontalVelocity = new Vector3(_rigidBody.linearVelocity.x, 0, _rigidBody.linearVelocity.z);
            _rigidBody.AddForce(-horizontalVelocity * _drag);
        }

        if (!_isAiming)
        {
            float speedMode = Input.GetKey(KeyCode.LeftShift) ? _runningMultiplier : 1f;
            _rigidBody.AddForce(_inputDirection * (_acceleration * speedMode));
        }

        if (_isJumpPressed)
        {
            _rigidBody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            _isJumpPressed = false;
            _isGrounded = false;
        }
    }

    #region Throwing Logic
    public void SetInValidThrowZone(bool isInZone)
    {
        _isInValidThrowZone = isInZone;
    }

    private void Aim()
    {
        if (Camera.main == null || !Camera.main.CompareTag("MainCamera")) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, _groundLayer))
        {
            _hasValidTarget = true;

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
        if (GameManager.Instance.Player.currentItem == null) return;

        GameObject projectile = GameManager.Instance.Player.currentItem.gameObject;
        GameManager.Instance.Player.UnequipForThrow();

        if (projectile.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = _calculatedVelocity;
        }

        if (_isInValidThrowZone && TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnThrow();
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
        float verticalInput = Input.GetAxisRaw("Vertical");

        if (verticalInput > 0.01f)
        {
            kinematicObj.AdvanceMovement(Time.fixedDeltaTime);
        }
        else if (verticalInput < -0.01f)
        {
            kinematicObj.ReverseMovement(Time.fixedDeltaTime);
        }
    }

    private void LateUpdate()
    {
        if (!_interactionScript || !transform.parent) return;

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
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (var contact in collision.contacts)
        {
            if (contact.normal.y > 0.7f)
            {
                _isGrounded = true;
                return;
            }
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance) GameManager.Instance.UnregisterPlayerMovement();
    }
}
