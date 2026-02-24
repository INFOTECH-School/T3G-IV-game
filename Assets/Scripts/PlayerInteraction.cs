using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public Player.PlayerState currentState = Player.PlayerState.Normal;
    private PushableObject currentTarget;
    [SerializeField] private GameObject pushText;
    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
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

        if (pushText) pushText.SetActive(false);
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

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance.CurrentGameState != GameManager.GameState.Gameplay) return;
        if (other.TryGetComponent(out PushableObject obj))
        {
            currentTarget = obj;
            if (pushText) pushText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (currentState != Player.PlayerState.Pushing && other.GetComponent<PushableObject>())
        {
            currentTarget = null;
            if (pushText) pushText.SetActive(false);
        }
    }
}