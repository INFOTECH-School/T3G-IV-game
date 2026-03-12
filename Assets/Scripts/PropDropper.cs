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
    
    [Header("Timeline Settings")]
    [Tooltip("The TimelineTrigger to play when the prop is dropped.")]
    public TimelineTrigger timelineTrigger;

    private bool _playerInTrigger;
    private bool _carInTrigger;

    private void Awake()
    {
        if (carObject == null)
        {
            Debug.LogError("[PropDropper] Car Object is not assigned. This component requires a reference to a KinematicObject.", this);
            this.enabled = false;
        }
    }

    private void Start()
    {
        if (propObject && fakePropObject)
        {
            propObject.SetActive(false);
            fakePropObject.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInTrigger = true;
            Debug.Log("[Dev Info] Player entered the trigger zone.");
            if (_carInTrigger)
            {
                Debug.Log($"[Player Info] You can now press '{dropKey}' to drop the prop!");
            }
        }
        // Check if the entering collider belongs to the assigned car object.
        else if (other.GetComponentInParent<KinematicObject>() == carObject)
        {
            _carInTrigger = true;
            Debug.Log($"[Dev Info] Car '{carObject.name}' entered the trigger. Prop drop is now enabled.");
            if (_playerInTrigger)
            {
                Debug.Log($"[Player Info] You can now press '{dropKey}' to drop the prop!");
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
        // Check if the exiting collider belongs to the assigned car object.
        else if (other.GetComponentInParent<KinematicObject>() == carObject)
        {
            _carInTrigger = false;
            Debug.Log($"[Dev Info] Car '{carObject.name}' exited the trigger. Prop drop is now disabled.");
        }
    }

    private void Update()
    {
        bool canDrop = _playerInTrigger && _carInTrigger;

        if (canDrop && Input.GetKeyDown(dropKey))
        {
            Debug.Log($"[Dev Info] Drop key '{dropKey}' pressed. Conditions met. Dropping prop.");
            DropProp();
        }
    }

    private void DropProp()
    {
        if (timelineTrigger)
        {
            timelineTrigger.Play();
            Debug.Log("[Dev Info] TimelineTrigger activated.");
        }
        else
        {
            if (propObject && fakePropObject)
            {
                propObject.SetActive(true);
                fakePropObject.SetActive(false);
                Debug.Log("[Player Info] Prop dropped!");
            }
            else
            {
                Debug.LogWarning("[Dev Info] Attempted to drop prop, but Prop prefab or fake prop object is not set.", this);
            }
        }
        
        Debug.Log("[Dev Info] PropDropper component disabled to prevent re-use.");
        enabled = false;
    }
}
