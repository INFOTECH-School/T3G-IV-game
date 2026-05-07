using UnityEngine;

/// <summary>
/// Animates vehicle wheels based on movement and modulates movement audio.
/// Attach this to the root of a car/vehicle (same object as KinematicObject if applicable).
/// </summary>
public class VehicleWheelAnimator : MonoBehaviour
{
    [Header("Wheel Configuration")]
    [Tooltip("The transforms of the wheels that should rotate.")]
    public Transform[] wheels;
    
    [Tooltip("The radius of the wheels for accurate rotation speed calculation.")]
    public float wheelRadius = 0.35f;
    
    [Tooltip("The local axis the wheels rotate around.")]
    public Vector3 rotationAxis = Vector3.forward;

    [Header("Audio Modulation")]
    [Tooltip("The AudioSource to modulate. If null, it will try to find one on this object.")]
    public AudioSource movementAudioSource;
    
    public float minPitch = 0.85f;
    public float maxPitch = 1.35f;
    public float minVolume = 0.05f;
    public float maxVolume = 1.0f;
    
    [Tooltip("The speed at which the car is considered to be at 'max' for audio modulation.")]
    public float referenceMaxSpeed = 4.0f;
    
    [Tooltip("How quickly the audio values transition.")]
    public float audioSmoothSpeed = 10f;

    [Header("Debug")]
    [SerializeField] private float currentSpeed;
    [SerializeField] private float currentDirection;

    private Vector3 _lastPosition;
    private bool _hasMovementSource;

    private void Start()
    {
        _lastPosition = transform.position;

        if (movementAudioSource == null)
        {
            // Try to get AudioSource from KinematicObject if it exists
            if (TryGetComponent<KinematicObject>(out var ko))
            {
                movementAudioSource = ko.moveAudioSource;
            }
            
            // Still null? try direct component
            if (movementAudioSource == null)
            {
                movementAudioSource = GetComponent<AudioSource>();
            }
        }

        // Initialize audio source if found
        if (movementAudioSource != null)
        {
            movementAudioSource.loop = true;
            if (movementAudioSource.playOnAwake && !movementAudioSource.isPlaying)
            {
                movementAudioSource.Play();
            }
        }

        // Auto-find wheels if none assigned
        if (wheels == null || wheels.Length == 0)
        {
            FindWheelsInChildren();
        }
    }

    private void FindWheelsInChildren()
    {
        var foundWheels = new System.Collections.Generic.List<Transform>();
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child.name.ToLower().Contains("wheel") && child != transform)
            {
                foundWheels.Add(child);
            }
        }
        wheels = foundWheels.ToArray();
        
        if (wheels.Length > 0)
        {
            Debug.Log($"[VehicleWheelAnimator] Auto-found {wheels.Length} wheels in {gameObject.name}");
        }
    }

    private void LateUpdate()
    {
        UpdateMovement();
        AnimateWheels();
        ModulateAudio();
        
        _lastPosition = transform.position;
    }

    private void UpdateMovement()
    {
        // Calculate displacement since last frame
        Vector3 displacement = transform.position - _lastPosition;
        float distanceThisFrame = displacement.magnitude;
        
        // Update current speed
        currentSpeed = distanceThisFrame / Time.deltaTime;
        
        // Update current direction (1 for forward, -1 for backward relative to car's forward)
        if (distanceThisFrame > 0.001f)
        {
            float dot = Vector3.Dot(displacement.normalized, transform.forward);
            currentDirection = dot >= 0 ? 1f : -1f;
        }
        else
        {
            // Keep previous direction when stopped to avoid flickering
        }
    }

    private void AnimateWheels()
    {
        if (wheels == null || wheels.Length == 0) return;

        float distanceThisFrame = (transform.position - _lastPosition).magnitude;
        if (distanceThisFrame < 0.0001f) return;

        // Rotation = (Distance / Circumference) * 360 degrees
        // Circumference = 2 * PI * Radius
        float rotationDegrees = (distanceThisFrame / (2f * Mathf.PI * wheelRadius)) * 360f;
        
        foreach (var wheel in wheels)
        {
            if (wheel == null) continue;
            
            // Rotate the wheel around its local axis
            wheel.Rotate(rotationAxis, rotationDegrees * currentDirection, Space.Self);
        }
    }

    private void ModulateAudio()
    {
        if (movementAudioSource == null) return;

        // Only modulate if it's supposed to be playing (handle KinematicObject stopping it)
        bool shouldBePlaying = currentSpeed > 0.05f;
        
        if (shouldBePlaying && !movementAudioSource.isPlaying)
        {
            movementAudioSource.Play();
        }
        else if (!shouldBePlaying && movementAudioSource.isPlaying && movementAudioSource.volume < 0.01f)
        {
            // Let the smoothing handle the volume fade out before stopping? 
            // Or just keep it playing at minVolume.
        }

        // Calculate target pitch and volume
        float speedFactor = Mathf.Clamp01(currentSpeed / referenceMaxSpeed);
        float targetPitch = Mathf.Lerp(minPitch, maxPitch, speedFactor);
        float targetVolume = shouldBePlaying ? Mathf.Lerp(minVolume, maxVolume, speedFactor) : 0f;

        // Apply smoothing
        movementAudioSource.pitch = Mathf.Lerp(movementAudioSource.pitch, targetPitch, Time.deltaTime * audioSmoothSpeed);
        movementAudioSource.volume = Mathf.Lerp(movementAudioSource.volume, targetVolume, Time.deltaTime * audioSmoothSpeed);
    }
}
