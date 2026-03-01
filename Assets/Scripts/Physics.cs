using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomFriction : MonoBehaviour
{
    [Header("Friction Settings")]
    [Tooltip("Im wyższa wartość, tym szybciej pocisk wyhamuje po upadku na ziemię.")]
    public float _groundDrag = 1.67f;

    private Rigidbody _rigidBody;
    private bool _isGrounded;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (_isGrounded)
        {
            Vector3 currentVelocity = _rigidBody.linearVelocity;
            Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
            
            Vector3 friction = -horizontalVelocity * _groundDrag;
            
            _rigidBody.AddForce(friction);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y > 0.5f)
            {
                _isGrounded = true;
                return;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        _isGrounded = false;
    }
}