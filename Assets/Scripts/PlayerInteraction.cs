using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public Player.PlayerState currentState = Player.PlayerState.Normal;
    private PushableObject currentTarget;
    private KinematicObject currentKinematicTarget;
    [SerializeField] private GameObject pushText;
    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
    
    void Update()
    {
        // Handle PushableObject interaction
        if (currentTarget != null && Input.GetKeyDown(KeyCode.X))
        {
            TogglePushMode();
        }
        
        // Handle KinematicObject interaction
        if (currentKinematicTarget != null && Input.GetKeyDown(KeyCode.X))
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
        if (currentTarget == null) return;

        currentState = Player.PlayerState.Pushing;

        // 1. Całkowite zamrożenie fizyki gracza i wyłączenie kolizji
        // Dzięki temu gracz nie "walczy" z ruchem klocka
        _rb.isKinematic = true; 
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.detectCollisions = false; 

        // 2. Podpięcie pod hierarchię klocka
        transform.SetParent(currentTarget.transform);
        
        // 3. Natychmiastowe wyrównanie do uchwytu
        transform.position = currentTarget.grabPoint.position;
        transform.rotation = currentTarget.grabPoint.rotation;

        if (pushText != null) pushText.SetActive(false);
    }

    void ExitPushState()
    {
        // 1. Rozłączenie hierarchii
        transform.SetParent(null);
        
        // 2. Przywrócenie fizyki i kolizji
        _rb.isKinematic = false;
        _rb.detectCollisions = true;

        currentState = Player.PlayerState.Normal;
    }
    
    void EnterKinematicState()
    {
        if (currentKinematicTarget == null) return;

        currentState = Player.PlayerState.Interacting;

        // 1. Freeze player physics completely
        _rb.isKinematic = true;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.detectCollisions = false;

        // 2. Notify kinematic object to start interaction
        currentKinematicTarget.StartInteraction(transform);

        if (pushText != null) pushText.SetActive(false);
    }

    void ExitKinematicState()
    {
        if (currentKinematicTarget == null) return;

        // 1. Notify kinematic object to stop interaction
        currentKinematicTarget.StopInteraction();

        // 2. Restore player physics
        _rb.isKinematic = false;
        _rb.detectCollisions = true;

        currentState = Player.PlayerState.Normal;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PushableObject obj))
        {
            currentTarget = obj;
            if (pushText != null) pushText.SetActive(true);
        }
        else if (other.TryGetComponent(out KinematicObject kinObj))
        {
            currentKinematicTarget = kinObj;
            if (pushText != null) pushText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (currentState != Player.PlayerState.Pushing && other.GetComponent<PushableObject>())
        {
            currentTarget = null;
            if (pushText != null) pushText.SetActive(false);
        }
        else if (currentState != Player.PlayerState.Interacting && other.GetComponent<KinematicObject>())
        {
            currentKinematicTarget = null;
            if (pushText != null) pushText.SetActive(false);
        }
    }
    
    // Expose current kinematic target for PlayerMovement
    public KinematicObject CurrentKinematicTarget => currentKinematicTarget;
}