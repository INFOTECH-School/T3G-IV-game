using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Ustawienia Ruchu")]
    public float _acceleration = 10f; 
    public float _runningMultiplier = 1.67f;
    public float _drag = 1.5f;
    public float _pushSpeed = 3f;
    
    private Rigidbody _rigidBody;
    private Vector3 _inputDirection;
    private readonly Quaternion _rotation = Quaternion.Euler(0, 45, 0);
    private PlayerInteraction _interactionScript;
    
    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _interactionScript = GetComponent<PlayerInteraction>();
        
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterPlayerMovement(this);
    }
    
    void Update()
    {
        if (_interactionScript != null && _interactionScript.currentState == Player.PlayerState.Pushing)
        {
            PushMovement();
        }
        else
        {
            HandleNormalInput();
        }
    }

    // KLUCZ DO SUKCESU: LateUpdate wykonuje się po wszystkich ruchach w Update.
    // Wymuszamy tu pozycję gracza, żeby nie "pływał" za klockiem.
    void LateUpdate()
    {
        if (_interactionScript != null && _interactionScript.currentState == Player.PlayerState.Pushing)
        {
            if (transform.parent != null)
            {
                // Szukamy komponentu na klocku, żeby pobrać grabPoint
                PushableObject pushable = transform.parent.GetComponent<PushableObject>();
                if (pushable != null && pushable.grabPoint != null)
                {
                    transform.position = pushable.grabPoint.position;
                    transform.rotation = pushable.grabPoint.rotation;
                }
            }
        }
    }

    private void HandleNormalInput()
    {
        var rawInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        _inputDirection = _rotation * rawInput;

        if (_inputDirection.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(_inputDirection);
        }
    }

    private void FixedUpdate()
    {
        if (_interactionScript != null && _interactionScript.currentState != Player.PlayerState.Pushing)
        {
            _rigidBody.isKinematic = false;
            ApplyNormalPhysics();
        }
    }

    void ApplyNormalPhysics()
    {
        Vector3 friction = -_rigidBody.linearVelocity * _drag;
        _rigidBody.AddForce(friction);
    
        float speedMode = Input.GetKey(KeyCode.LeftShift) ? _runningMultiplier : 1f;
        _rigidBody.AddForce(_inputDirection * (_acceleration * speedMode));
    }

    void PushMovement()
    {
        if (transform.parent != null)
        {
            // Używamy GetAxisRaw("Vertical"), czyli W/S
            float verticalInput = Input.GetAxisRaw("Vertical");
        
            if (Mathf.Abs(verticalInput) < 0.01f) return;

            // KLUCZOWA ZMIANA:
            // Zamiast brać 'forward' klocka, bierzemy 'forward' gracza.
            // Ponieważ gracz patrzy na klocek, jego forward to zawsze oś "pchania/ciągnięcia".
            Vector3 moveDirection = transform.forward * verticalInput;

            // Poruszamy klockiem (rodzicem) w stronę, w którą patrzy gracz
            transform.parent.position += moveDirection * _pushSpeed * Time.deltaTime;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null) 
            GameManager.Instance.UnregisterPlayerMovement();
    }
}