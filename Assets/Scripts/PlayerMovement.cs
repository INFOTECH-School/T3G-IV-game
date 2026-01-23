using System;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    public float _acceleration = 10f;
    public float _runningMultiplier = 1.67f;
    public float _drag = 1.5f;
    public float _jumpForce = 5f;

    private Rigidbody _rigidBody;
    private Vector3 _inputDirection;
    private Vector3 _currentVelocity;
    private bool _isGrounded;
    private bool _isJumpPressed;
    
    private readonly Quaternion _rotation = Quaternion.Euler(0, 45, 0);

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        var rawInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        _inputDirection = _rotation * rawInput;
        
        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        {
            _isJumpPressed = true;
        }
    }

    private void FixedUpdate()
    {
    /* friction */
        Vector3 currentVelocity = _rigidBody.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
        var friction = -horizontalVelocity * _drag;
        
        _rigidBody.AddForce(friction);
        
    /* movement */
        
        float speedMode = Input.GetKey(KeyCode.LeftShift) ? _runningMultiplier : 1f;
        _rigidBody.AddForce(_inputDirection * (_acceleration * speedMode));
        
    /* jumping */
        if (_isJumpPressed)
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
}