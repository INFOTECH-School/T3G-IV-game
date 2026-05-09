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
    private TrailRenderer trailRenderer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trailRenderer = GetComponent<TrailRenderer>();
        
        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
            Debug.Log($"[ThrowablePhysics] Awake: TrailRenderer found and disabled on {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"[ThrowablePhysics] Awake: No TrailRenderer found on {gameObject.name}!");
        }

        // Store the initial drag values set in the Inspector (which should be 0)
        initialDrag = rb.linearDamping;
        initialAngularDrag = rb.angularDamping;
    }

    // This is called when the player picks up the item.
    public void OnPickup()
    {
        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }

        // Reset to initial values in case it was previously on the ground
        rb.linearDamping = initialDrag;
        rb.angularDamping = initialAngularDrag;
    }

    // This is called the moment the object is thrown.
    public void OnThrow()
    {
        Debug.Log($"[ThrowablePhysics] OnThrow called on {gameObject.name}");
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
            trailRenderer.emitting = true;
            Debug.Log($"[ThrowablePhysics] OnThrow: TrailRenderer cleared and emitting = true");
        }
        else
        {
            Debug.LogWarning($"[ThrowablePhysics] OnThrow: Cannot emit, TrailRenderer is NULL on {gameObject.name}!");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Stop drawing the trail on impact, unless we're just colliding with the player who threw it
        if (!collision.gameObject.CompareTag("Player"))
        {
            if (trailRenderer != null)
            {
                trailRenderer.emitting = false;
                Debug.Log($"[ThrowablePhysics] OnCollisionEnter: TrailRenderer emitting = false on {gameObject.name}");
            }
        }

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
