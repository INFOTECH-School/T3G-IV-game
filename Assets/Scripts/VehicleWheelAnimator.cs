using UnityEngine;

/// <summary>
/// Animates vehicle wheels based on movement and modulates movement audio.
/// Attach this to the root of a car/vehicle (same object as KinematicObject if applicable).
/// </summary>
public class VehicleWheelAnimator : MonoBehaviour
{
    public enum MovementReference { Forward, Up, Right }
    
    [Header("Wheel Configuration")]
    [Tooltip("The transforms of the wheels that should rotate.")]
    public Transform[] wheels;
    
    [Tooltip("The radius of the wheels for accurate rotation speed calculation.")]
    public float wheelRadius = 0.35f;
    
    [Tooltip("The local axis the wheels rotate around. Z (0,0,1) is now default.")]
    public Vector3 rotationAxis = Vector3.forward;

    [Tooltip("Which local axis of the vehicle determines forward movement?")]
    public MovementReference movementReference = MovementReference.Forward;

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
    private Quaternion[] _initialLocalRotations;
    private float _accumulatedRotation;
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

        // Store initial rotations for stable Z-only rotation
        if (wheels != null)
        {
            _initialLocalRotations = new Quaternion[wheels.Length];
            for (int i = 0; i < wheels.Length; i++)
            {
                if (wheels[i] != null) _initialLocalRotations[i] = wheels[i].localRotation;
            }
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
        
        // Update current direction based on chosen reference
        if (distanceThisFrame > 0.001f)
        {
            Vector3 referenceDir = transform.forward;
            switch (movementReference)
            {
                case MovementReference.Up: referenceDir = transform.up; break;
                case MovementReference.Right: referenceDir = transform.right; break;
            }

            float dot = Vector3.Dot(displacement.normalized, referenceDir);
            currentDirection = dot >= 0 ? 1f : -1f;
        }
    }

    private void AnimateWheels()
    {
        if (wheels == null || wheels.Length == 0) return;

        float distanceThisFrame = (transform.position - _lastPosition).magnitude;
        
        // Update accumulated rotation even if distance is small to maintain consistency
        // Rotation = (Distance / Circumference) * 360 degrees
        float rotationStep = (distanceThisFrame / (2f * Mathf.PI * wheelRadius)) * 360f;
        _accumulatedRotation += rotationStep * currentDirection;
        
        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i] == null) continue;
            
            // Apply rotation strictly around the chosen axis relative to initial rotation
            // This prevents Y/X "drift" or wobble that can happen with incremental .Rotate()
            wheels[i].localRotation = _initialLocalRotations[i] * Quaternion.AngleAxis(_accumulatedRotation, rotationAxis);
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
