using System;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float _acceleration = 10f;
    public float _runningMultiplier = 1.67f;
    public float _drag = 1.5f;
    public float _jumpForce = 5f;
    
    [Header("Throw Settings")]
    public int _lineSegments = 30;
    public float _timeBetweenDots = 0.1f;
    public GameObject _itemPrefab;
    public float _timeToTarget = 1f;
    public LayerMask _groundLayer;

    private bool _hasValidTarget = false;
    private Vector3 _calculatedVelocity;
    
    [Header("References")]
    public LineRenderer _lineRenderer; // add this in inspector
    public Transform _throwingPoint; // probly players hand
    
    private Rigidbody _rigidBody;
    private Vector3 _inputDirection;
    private Vector3 _currentVelocity;
    private Vector3 _targetPosition;
    private bool _isGrounded;
    private bool _isJumpPressed;
    private bool _isAiming = false;
    private int _speedHash;
    private int _groundedHash;
    private int _runningHash;

    private Animator _animator;
    private readonly Quaternion _rotation = Quaternion.Euler(0, 45, 0);

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        
        _speedHash = Animator.StringToHash("Speed");
        _groundedHash = Animator.StringToHash("isGrounded");
        _runningHash = Animator.StringToHash("isRunning");
        
        GameManager.Instance.RegisterPlayerMovement(this);
        
    }

    void Update()
    {
        var rawInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        _inputDirection = _rotation * rawInput;
        
        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        {
            _isJumpPressed = true;
        }
        Vector3 horizontalVel = new Vector3(_rigidBody.linearVelocity.x, 0, _rigidBody.linearVelocity.z);

        if (Input.GetMouseButtonDown(1) && _isGrounded && horizontalVel.magnitude < 0.1f)
        {
            _isAiming = true;
            _rigidBody.linearVelocity = new Vector3(0, _rigidBody.linearVelocity.y, 0);
        }

        if (Input.GetMouseButton(1) && _isAiming) { Aim(); }

        if (Input.GetMouseButtonUp(1) && _isAiming)
        {
            _isAiming = false;
            HideTrajectory();
            
            if (_hasValidTarget) { ThrowItem(); }
        }

        if (_animator)
        {
            Vector3 horizontalVelocity = new Vector3(_rigidBody.linearVelocity.x, 0, _rigidBody.linearVelocity.z);
            float currentSpeed = horizontalVelocity.magnitude;

            if (currentSpeed < 0.1f) currentSpeed = 0.0f;
            _animator.SetFloat(_speedHash, currentSpeed);
            _animator.SetBool(_groundedHash, _isGrounded);
            _animator.SetBool(_runningHash, Input.GetKey(KeyCode.LeftShift));
        }
    }
    
    
    private void FixedUpdate()
    {
        Vector3 currentVelocity = _rigidBody.linearVelocity; /* friction */
        Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
        var friction = -horizontalVelocity * _drag;
        
        _rigidBody.AddForce(friction);

        if (!_isAiming)
        {
            float speedMode = Input.GetKey(KeyCode.LeftShift) ? _runningMultiplier : 1f; /* movement */
            _rigidBody.AddForce(_inputDirection * (_acceleration * speedMode));
        }

        if (_isJumpPressed && !_isAiming) /* jumping */
        {
            _rigidBody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            _isJumpPressed = false;
            _isGrounded = false;
        }
    }

    void OnCollisionStay(Collision collision)
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

    private void DrawTrajectory(Vector3 initialVelocity)
    {
        _lineRenderer.positionCount = _lineSegments;
        Vector3[] positions = new Vector3[_lineSegments];
        
        for (int i = 0; i <  _lineSegments; i++)
        {
            float time = i *  _timeBetweenDots;
            
            // scheme: P = P0 + V*t + 0.5*g*t^2
            Vector3 position = _throwingPoint.position + (initialVelocity * time) + (0.5f * Physics.gravity * time * time);
            
            positions[i] = position;
        }
        
        _lineRenderer.SetPositions(positions);
    }

    private void HideTrajectory()
    {
        _lineRenderer.positionCount = 0;
    }

    private void Aim()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, _groundLayer))
        {
            _hasValidTarget = true; // Zaznaczamy, że mamy poprawny cel
            
            Vector3 lookDirection = hit.point - transform.position;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }

            _calculatedVelocity = CalculateVelocity(_throwingPoint.position, hit.point, _timeToTarget);
            DrawTrajectory(_calculatedVelocity);
        }
        else
        {
            // Jeśli wycelowaliśmy w niebo/nicość:
            _hasValidTarget = false;
            HideTrajectory(); // Chowamy linię, żeby gracz wiedział, że rzut się nie uda
        }
    }

    private Vector3 CalculateVelocity(Vector3 start, Vector3 target, float time)
    {
        Vector3 distanceXZ = new Vector3(target.x - start.x, 0, target.z - start.z);
        float distanceY = target.y - start.y;

        /* Physics :O*/
        // v = s / t
        Vector3 velocityXZ = distanceXZ / time;
        // v = (s - 1 / 2gt2)t
        float velocityY = (distanceY - 0.5f / 2 * Physics.gravity.y * time * time) / time;
        
        Vector3 finalVelocity = velocityXZ;
        finalVelocity.y = velocityY;

        return finalVelocity;
    }

    private void ThrowItem()
    {
        GameObject projectile = Instantiate(_itemPrefab, _throwingPoint.position, Quaternion.identity);

        if (projectile.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.linearVelocity = _calculatedVelocity;
        } 
    }
    
    private void OnDestroy()
    {
        if (GameManager.Instance) GameManager.Instance.UnregisterPlayerMovement();
    }
}