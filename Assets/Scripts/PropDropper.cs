using UnityEngine;

public class PropDropper : MonoBehaviour
{
    [Header("Prop Settings")]
    public GameObject propObject;
    public GameObject fakePropObject;

    [Tooltip("The key to press to drop the prop.")]
    public KeyCode dropKey = KeyCode.E;

    [Header("Car Reference")]
    [Tooltip("The KinematicObject representing the car. This is required to check the car's state.")]
    public KinematicObject carObject;

    private bool _playerInTrigger = false;
    private bool _carHasReachedTarget = false;

    private void Awake()
    {
        if (carObject == null)
        {
            Debug.LogError("[PropDropper] Car Object is not assigned. This component requires a reference to a KinematicObject.", this);
            this.enabled = false;
            return;
        }

        Debug.Log($"[Dev Info] PropDropper initialized. Subscribing to OnTargetReached event for car '{carObject.name}'.");
        carObject.OnTargetReached += HandleCarTargetReached;
    }

    private void Start()
    {
        propObject.SetActive(false);
        fakePropObject.SetActive(true);
    }

    private void OnDestroy()
    {
        if (carObject != null)
        {
            carObject.OnTargetReached -= HandleCarTargetReached;
            Debug.Log($"[Dev Info] PropDropper destroyed. Unsubscribed from OnTargetReached event for car '{carObject.name}'.");
        }
    }

    private void HandleCarTargetReached()
    {
        _carHasReachedTarget = true;
        Debug.Log($"[Dev Info] Received OnTargetReached event from '{carObject.name}'. Prop drop is now enabled.");
        if (_playerInTrigger)
        {
            Debug.Log("[Player Info] You can now press 'E' to drop the prop!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInTrigger = true;
            Debug.Log("[Dev Info] Player entered the trigger zone.");
            if (_carHasReachedTarget)
            {
                Debug.Log("[Player Info] You can now press 'E' to drop the prop!");
            }
        }
        else if (other.gameObject == carObject.gameObject)
        {
            if (_carHasReachedTarget)
            {
                Debug.Log($"[Dev Info] Car '{carObject.name}' re-entered the trigger on its return trip. Disabling drop ability.");
                _carHasReachedTarget = false;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInTrigger = false;
            Debug.Log("[Dev Info] Player exited the trigger zone.");
        }
    }

    private void Update()
    {
        bool canDrop = _playerInTrigger && _carHasReachedTarget;

        if (canDrop && Input.GetKeyDown(dropKey))
        {
            Debug.Log($"[Dev Info] Drop key '{dropKey}' pressed. Conditions met. Dropping prop.");
            DropProp();
        }
    }

    private void DropProp()
    {
        if (propObject != null && fakePropObject != null)
        {
            propObject.SetActive(true);
            fakePropObject.SetActive(false);
            Debug.Log("[Player Info] Prop dropped!");
            Debug.Log("[Dev Info] PropDropper component disabled to prevent re-use.");
            
            enabled = false; 
        }
        else
        {
            Debug.LogWarning("[Dev Info] Attempted to drop prop, but Prop prefab or fake prop object is not set.", this);
        }
    }
}
