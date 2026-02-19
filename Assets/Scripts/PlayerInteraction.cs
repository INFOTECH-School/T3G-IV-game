using UnityEngine;
using TMPro; // Required for TextMeshPro

public class PlayerInteraction : MonoBehaviour
{
    public Player.PlayerState currentState = Player.PlayerState.Normal;
    
    private PushableObject currentTarget;
    private KinematicObject currentKinematicTarget; // Added from Script 2
    
    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI interactionText; // Prioritized TMP over standard GameObject
    
    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        UpdateUI(); // Ensure text is hidden at start
    }
    
    void Update()
    {
        // GameManager check from Script 1
        if (GameManager.Instance.CurrentGameState != GameManager.GameState.Gameplay) return;

        // Handle PushableObject interaction
        if (currentTarget && Input.GetKeyDown(KeyCode.X))
        {
            TogglePushMode();
        }
        // Handle KinematicObject interaction (uses 'else if' to prevent double inputs if overlapping)
        else if (currentKinematicTarget && Input.GetKeyDown(KeyCode.X))
        {
            ToggleKinematicMode();
        }
    }

    private void TogglePushMode()
    {
        if (currentState == Player.PlayerState.Normal) EnterPushState();
        else ExitPushState();
    }

    private void ToggleKinematicMode()
    {
        if (currentState == Player.PlayerState.Normal) EnterKinematicState();
        else ExitKinematicState();
    }

    void EnterPushState()
    {
        if (!currentTarget) return;

        currentState = Player.PlayerState.Pushing;

        // 1. Freeze Physics
        _rb.isKinematic = true; 
        _rb.linearVelocity = Vector3.zero; 
        _rb.angularVelocity = Vector3.zero;
        _rb.detectCollisions = false; 

        // 2. Parent and Snap
        transform.SetParent(currentTarget.transform);
        transform.position = currentTarget.grabPoint.position;
        transform.rotation = currentTarget.grabPoint.rotation;
        
        // 3. Update UI
        UpdateUI();
    }

    void ExitPushState()
    {
        // 1. Unparent
        transform.SetParent(null);
        
        // 2. Restore Physics
        _rb.isKinematic = false;
        _rb.detectCollisions = true;

        currentState = Player.PlayerState.Normal;

        // 3. Update UI
        UpdateUI();
    }

    void EnterKinematicState()
    {
        if (!currentKinematicTarget) return;

        currentState = Player.PlayerState.Interacting;

        // 1. Freeze player physics completely
        _rb.isKinematic = true;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.detectCollisions = false;

        // 2. Notify kinematic object to start interaction
        currentKinematicTarget.StartInteraction(transform);

        // 3. Update UI
        UpdateUI();
    }

    void ExitKinematicState()
    {
        if (!currentKinematicTarget) return;

        // 1. Notify kinematic object to stop interaction
        currentKinematicTarget.StopInteraction();

        // 2. Restore player physics
        _rb.isKinematic = false;
        _rb.detectCollisions = true;

        currentState = Player.PlayerState.Normal;

        // 3. Update UI
        UpdateUI();
    }

    // --- UI LOGIC ---

    private void UpdateUI()
    {
        if (!interactionText) return;

        if (currentState == Player.PlayerState.Pushing)
        {
            // PUSHING STATE TEXT
            interactionText.text = "Hold <color=yellow>[W]</color> / <color=yellow>[S]</color> to Push\nPress <color=red>[X]</color> to Let Go";
            interactionText.gameObject.SetActive(true);
        }
        else if (currentState == Player.PlayerState.Interacting)
        {
            // KINEMATIC INTERACTING STATE TEXT
            interactionText.text = "Press <color=red>[X]</color> to Stop";
            interactionText.gameObject.SetActive(true);
        }
        else if (currentTarget)
        {
            // HOVER PUSHABLE TEXT
            interactionText.text = "Press <color=green>[X]</color> to Grab";
            interactionText.gameObject.SetActive(true);
        }
        else if (currentKinematicTarget)
        {
            // HOVER KINEMATIC TEXT
            interactionText.text = "Press <color=green>[X]</color> to Interact";
            interactionText.gameObject.SetActive(true);
        }
        else
        {
            // NO INTERACTION
            interactionText.gameObject.SetActive(false);
        }
    }

    // --- TRIGGERS ---

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
            currentKinematicTarget = kinObj;
            UpdateUI();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Don't lose the target if we are currently holding/interacting with it
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
    
    // Expose current kinematic target for PlayerMovement
    public KinematicObject CurrentKinematicTarget => currentKinematicTarget;
}