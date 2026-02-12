using UnityEngine;
using TMPro; // Required for TextMeshPro

public class PlayerInteraction : MonoBehaviour
{
    public Player.PlayerState currentState = Player.PlayerState.Normal;
    private PushableObject currentTarget;
    
    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI interactionText; // Drag your TMP Text here
    
    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        UpdateUI(); // Ensure text is hidden at start
    }
    
    void Update()
    {
        if (GameManager.Instance.CurrentGameState != GameManager.GameState.Gameplay) return;
        if (currentTarget && Input.GetKeyDown(KeyCode.X))
        {
            TogglePushMode();
        }
    }

    private void TogglePushMode()
    {
        if (currentState == Player.PlayerState.Normal) EnterPushState();
        else ExitPushState();
    }

    void EnterPushState()
    {
        if (!currentTarget) return;

        currentState = Player.PlayerState.Pushing;

        // 1. Freeze Physics
        _rb.isKinematic = true; 
        _rb.linearVelocity = Vector3.zero; // Note: In Unity 6 this is linearVelocity, older versions use velocity
        _rb.angularVelocity = Vector3.zero;
        _rb.detectCollisions = false; 

        // 2. Parent and Snap
        transform.SetParent(currentTarget.transform);
        transform.position = currentTarget.grabPoint.position;
        transform.rotation = currentTarget.grabPoint.rotation;
        
        // 3. Update UI to show Pushing controls
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

        // 3. Update UI to show Grab controls again
        UpdateUI();
    }

    // --- UI LOGIC ---

    private void UpdateUI()
    {
        if (interactionText == null) return;

        if (currentState == Player.PlayerState.Pushing)
        {
            // PUSHING STATE TEXT
            // Uses color tags for clearer instructions
            interactionText.text = "Hold <color=yellow>[W]</color> / <color=yellow>[S]</color> to Push\nPress <color=red>[X]</color> to Let Go";
            interactionText.gameObject.SetActive(true);
        }
        else if (currentTarget != null)
        {
            // HOVER STATE TEXT
            interactionText.text = "Press <color=green>[X]</color> to Grab";
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
            UpdateUI(); // Trigger UI immediately when walking up to object
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Don't lose the target if we are currently holding it!
        if (currentState != Player.PlayerState.Pushing && other.GetComponent<PushableObject>())
        {
            currentTarget = null;
            UpdateUI(); // Hide UI
        }
    }
}