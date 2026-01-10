using System;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    public float _acceleration = 10f; 
    public float runningMultiplier = 1.67f;
    public float _drag = 1.5f;
    
    private Rigidbody _rigidBody;
    private Vector3 _inputDirection; // direction where player moves
    private Vector3 _currentVelocity; 
    
    private readonly Quaternion _rotation = Quaternion.Euler(0, 45, 0);
    
    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }
    
    void Update()
    {
        var rawInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        
        _inputDirection = _rotation * rawInput;
    }

    private void FixedUpdate()
    {
        Vector3 friction = -_rigidBody.linearVelocity * _drag ;
        _rigidBody.AddForce(friction);
            
        float speedMode = Input.GetKey(KeyCode.LeftShift) ? runningMultiplier : 1f;
        _rigidBody.AddForce(_inputDirection * (_acceleration * speedMode));
    }
}