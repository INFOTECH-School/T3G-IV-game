using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public Player.PlayerState currentState = Player.PlayerState.Normal;
    
    private PushableObject currentTarget;
    private KinematicObject currentKinematicTarget;
    
    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI interactionText; 
    
    private Rigidbody _rb;
    [SerializeField] private BoxCollider _collider;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        UpdateUI(); // Ensure text is hidden at start
    }
    
    void Update()
    {
        // GameManager check from local script
        if (GameManager.Instance.CurrentGameState != GameManager.GameState.Gameplay) return;

        // A single, unified check for the 'X' key press on any valid target.
        if (Input.GetKeyDown(KeyCode.X) && (currentTarget != null || currentKinematicTarget != null))
        {
            // Notify the tutorial manager that an interaction has occurred.
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnInteractPressed();
            }

            // Now, decide which type of interaction to perform.
            if (currentTarget)
            {
                TogglePushMode();
            }
            else if (currentKinematicTarget)
            {
                ToggleKinematicMode();
            }
        }
    }

    private void TogglePushMode()
    {
        if (currentState == Player.PlayerState.Normal)
        {
            EnterPushState();
        }
        else if (currentState == Player.PlayerState.Pushing)
        {
            if (IsExitLocationSafe())
            {
                ExitPushState();
            }
            else
            {
                Debug.LogWarning("Cannot let go here, position is obstructed!");
            }
        }
    }

    public void ToggleKinematicMode()
    {
        if (currentState == Player.PlayerState.Normal) EnterKinematicState();
        else ExitKinematicState();
    }

    void EnterPushState()
    {
        if (!currentTarget) return;

        currentState = Player.PlayerState.Pushing;

        _rb.isKinematic = true; 
        _rb.linearVelocity = Vector3.zero; 
        _rb.angularVelocity = Vector3.zero;
        _rb.detectCollisions = false; 

        transform.SetParent(currentTarget.transform);
        transform.position = currentTarget.grabPoint.position;
        transform.rotation = currentTarget.grabPoint.rotation;
        
        UpdateUI();
        
        // The OnPushing event is now handled by PlayerMovement when the object is actually moved.
    }

    private bool IsExitLocationSafe()
    {
        if (!_collider)
        {
            Debug.LogError("Player has no CapsuleCollider to perform a safety check!");
            return false;
        }

        Vector3 worldCenter = transform.TransformPoint(_collider.center);
        Vector3 halfExtents = Vector3.Scale(_collider.size, transform.lossyScale) * 0.5f;
        Quaternion orientation = transform.rotation;
        int layerMask = ~(1 << gameObject.layer | 1 << currentTarget.gameObject.layer);
        bool isOverlapping = Physics.CheckBox(worldCenter, halfExtents, orientation, layerMask, QueryTriggerInteraction.Ignore);

        return !isOverlapping;
    }

    void ExitPushState()
    {
        transform.SetParent(null);
        
        _rb.isKinematic = false;
        _rb.detectCollisions = true;

        currentState = Player.PlayerState.Normal;
        
        UpdateUI();
    }

    void EnterKinematicState()
    {
        if (!currentKinematicTarget) return;

        currentState = Player.PlayerState.Interacting;

        _rb.isKinematic = true;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.detectCollisions = false;

        currentKinematicTarget.StartInteraction(transform);
        
        UpdateUI();
    }

    void ExitKinematicState()
    {
        if (!currentKinematicTarget) return;

        currentKinematicTarget.StopInteraction();

        _rb.isKinematic = false;
        _rb.detectCollisions = true;

        currentState = Player.PlayerState.Normal;
        
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (!interactionText) return;

        if (currentState == Player.PlayerState.Pushing)
        {
            interactionText.text = "Hold <color=yellow>[W]</color> / <color=yellow>[S]</color> to Push\nPress <color=red>[X]</color> to Let Go";
            interactionText.gameObject.SetActive(true);
        }
        else if (currentState == Player.PlayerState.Interacting)
        {
            interactionText.text = "Press <color=red>[X]</color> to Stop";
            interactionText.gameObject.SetActive(true);
        }
        else if (currentTarget)
        {
            interactionText.text = "Press <color=green>[X]</color> to Grab";
            interactionText.gameObject.SetActive(true);
        }
        else if (currentKinematicTarget)
        {
            interactionText.text = "Press <color=green>[X]</color> to Interact";
            interactionText.gameObject.SetActive(true);
        }
        else
        {
            interactionText.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance.CurrentGameState != GameManager.GameState.Gameplay) return;

        if (other.TryGetComponent(out PushableObject obj))
        {
            currentTarget = obj;
            UpdateUI(); 
        }
        else if (other.TryGetComponent(out KinematicObject kinObj))
        {
            if (kinObj.CanInteract)
            {
                currentKinematicTarget = kinObj;
                UpdateUI();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (currentState != Player.PlayerState.Pushing && other.GetComponent<PushableObject>())
        {
            currentTarget = null;
            UpdateUI(); 
        }
        else if (currentState != Player.PlayerState.Interacting && other.GetComponent<KinematicObject>())
        {
            currentKinematicTarget = null;
            UpdateUI();
        }
    }
    
    public KinematicObject CurrentKinematicTarget => currentKinematicTarget;
}
