using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ThrowablePhysics : MonoBehaviour
{
    [Header("Ground Collision Settings")]
    [Tooltip("The layer(s) that should be considered 'ground'.")]
    public LayerMask groundLayer; 

    [Tooltip("The drag value to apply when the object is on the ground.")]
    public float groundDrag = 5f;

    [Tooltip("The angular drag value to apply when on the ground to stop spinning.")]
    public float groundAngularDrag = 5f;

    private Rigidbody rb;
    private float initialDrag;
    private float initialAngularDrag;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        initialDrag = rb.linearDamping;
        initialAngularDrag = rb.angularDamping;
    }

    // This is called when the player picks up the item.
    public void OnPickup()
    {
        // Reset to initial values in case it was previously on the ground
        rb.linearDamping = initialDrag;
        rb.angularDamping = initialAngularDrag;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the object we collided with is on the ground layer
        if ((groundLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            // If it is, apply the high "ground" drag values
            rb.linearDamping = groundDrag;
            rb.angularDamping = groundAngularDrag;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // Optional: If the object is somehow launched off the ground again,
        // reset its drag so it can fly freely.
        if ((groundLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            rb.linearDamping = initialDrag;
            rb.angularDamping = initialAngularDrag;
        }
    }
}
