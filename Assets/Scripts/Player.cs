using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public enum PlayerState { Normal, Pushing, Interacting }

    [Header("Settings")]
    public Transform holdPoint; 
    public float itemScale = 0.5f;
    public LayerMask itemLayer;

    [Header("State")]
    public Item currentItem;
    public bool IsPickingUp { get; private set; }
    
    // Track nearby objects
    private Item nearbyItem;
    private Basket nearbyBasket; 
    private BrokenWheel nearbyWheel;

    // Memories for the "Return to Shelf" feature
    private Vector3 originalPos;
    private Quaternion originalRot;
    private Vector3 originalScale;
    
    [Header("Sounds")]
    public AudioClip pickupClip;
    
    [Header("Degradation System")]
    public List<PlayerDegradationState> degradationStates;
    public int currentDegradationIndex { get; private set; } = 0;
    
    [SerializeField] private Animator _animator;
    
    private void Start()
    {
        if (GameManager.Instance) GameManager.Instance.RegisterPlayer(this);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance) GameManager.Instance.UnregisterPlayer();
    }
    
    void Update()
    {
        // === INPUT LOGIC ===
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentItem)
            {
                if (nearbyBasket && nearbyBasket.allowedItems.Contains(currentItem)) PlaceInBasket();
                else if (nearbyWheel && currentItem == nearbyWheel.requiredItem) FixWheel();
                else if (nearbyItem)
                {
                    // Player tries to pick up a new item while already holding one
                    var feedback = currentItem.GetComponentInChildren<InteractionFeedback>();
                    if (feedback) feedback.ShowErrorFeedback();
                }
            }
            else if (nearbyItem)
            {
                Equip(nearbyItem);
            }
        }
        
        // === GUIDE LOGIC ===
        if (nearbyBasket && nearbyBasket.guide)
        {
            bool isCorrectItem = currentItem != null && nearbyBasket.allowedItems.Contains(currentItem);
            if (nearbyBasket.guide.activeSelf != isCorrectItem)
            {
                nearbyBasket.guide.SetActive(isCorrectItem);
            }
        }
    }

    // === ACTIONS ===

    public void Equip(Item itemToEquip)
    {
        if (!itemToEquip) return;

        if (_animator)
        {
            _animator.SetTrigger("Pickup");
            StartCoroutine(PickupRoutine());
        }
        Debug.Log("Player picked up an item");
        Debug.Log(_animator.GetCurrentAnimatorStateInfo(0).IsName("Pickup"));
        if (pickupClip)
        {
            AudioSource.PlayClipAtPoint(pickupClip, transform.position);
        }

        currentItem = itemToEquip;
        if (currentItem.CompareTag("Throwable"))
        {
            currentItem.GetComponent<ThrowablePhysics>().OnPickup();
        }
        originalPos = currentItem.transform.position;
        originalRot = currentItem.transform.rotation;
        originalScale = currentItem.transform.localScale;

        var rb = currentItem.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
        
        var colliders = currentItem.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        currentItem.transform.SetParent(holdPoint); 
        currentItem.transform.localPosition = Vector3.zero;
        currentItem.transform.localRotation = Quaternion.identity;
        currentItem.transform.localScale = new Vector3(itemScale, itemScale, itemScale);
    }

    public void Unequip()
    {
        if (!currentItem) return;

        currentItem.transform.SetParent(null);
        currentItem.transform.position = originalPos; 
        currentItem.transform.rotation = originalRot;
        currentItem.transform.localScale = originalScale;

        ResetItemPhysics();
        currentItem = null;
    }

    private System.Collections.IEnumerator PickupRoutine()
    {
        IsPickingUp = true;
        yield return null;
        yield return null;
        
        if (_animator)
        {
            while (_animator.GetCurrentAnimatorStateInfo(0).IsName("Pickup") || 
                  (_animator.IsInTransition(0) && _animator.GetNextAnimatorStateInfo(0).IsName("Pickup")))
            {
                yield return null;
            }
        }
        IsPickingUp = false;
    }
    
    public void UnequipForThrow()
    {
        if (!currentItem) return;

        currentItem.transform.SetParent(null);
        currentItem.transform.localScale = originalScale;
        
        var rb = currentItem.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = false;

        var colliders = currentItem.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = true;
        }

        currentItem = null;
    }

    public void PlaceInBasket()
    {
        if (!currentItem || !nearbyBasket) return;

        currentItem.transform.localScale = originalScale;
        nearbyBasket.ReceiveItem(currentItem);
        currentItem = null;
    }

    private void FixWheel()
    {
        if (!currentItem || !nearbyWheel || currentItem != nearbyWheel.requiredItem) return;

        nearbyWheel.Fix();
        Destroy(currentItem.gameObject);
        currentItem = null;
    }

    private void ResetItemPhysics()
    {
        if (!currentItem) return;
        var col = currentItem.GetComponent<Collider>();
        if (col) col.enabled = true;
    }

    // === TRIGGERS ===

    private void OnTriggerEnter(Collider other)
    {
        // Check Item
        if ((itemLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            var itemScript = other.GetComponent<Item>();
            if (itemScript) nearbyItem = itemScript;
        }

        // Check Basket
        var basketScript = other.GetComponent<Basket>();
        if (basketScript) nearbyBasket = basketScript;
        
        // Check BrokenWheel
        var wheelScript = other.GetComponent<BrokenWheel>();
        if (wheelScript) nearbyWheel = wheelScript;
    }

    private void OnTriggerExit(Collider other)
    {
        // Clear Item
        if ((itemLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            var itemScript = other.GetComponent<Item>();
            if (itemScript == nearbyItem) nearbyItem = null;
        }

        // Clear Basket
        var basketScript = other.GetComponent<Basket>();
        if (basketScript && basketScript == nearbyBasket)
        {
            if (nearbyBasket.guide) nearbyBasket.guide.SetActive(false);
            nearbyBasket = null;
        }
        
        // Clear BrokenWheel
        var wheelScript = other.GetComponent<BrokenWheel>();
        if (wheelScript && wheelScript == nearbyWheel) nearbyWheel = null;
    }

    public void Degrade()
    {
        if (Utils.IsCutsceneGhostModeActive)
        {
            Debug.Log("Skipping Degrade() during GhostPlay mode.");
            return;
        }

        if (degradationStates == null || degradationStates.Count == 0)
        {
            Debug.LogWarning("No degradation states set up in the Player script!");
            return;
        }
        
        if (currentDegradationIndex + 1 < degradationStates.Count)
        {
            // Turn off current state object
            if (degradationStates[currentDegradationIndex].stateObject)
                degradationStates[currentDegradationIndex].stateObject.SetActive(false);
            
            // Advance index
            currentDegradationIndex++;
            var newState = degradationStates[currentDegradationIndex];
            
            // Turn on new state object
            if (newState.stateObject)
                newState.stateObject.SetActive(true);
            
            // Reassign Player references
            if (newState.holdPoint) holdPoint = newState.holdPoint;
            if (newState.animator) _animator = newState.animator;
            
            // Reassign PlayerMovement references
            var movement = GetComponent<PlayerMovement>();
            if (movement)
            {
                movement.SetDegradationState(newState.throwingPoint, newState.animator);
            }
            
            // If the player is currently holding an item, instantly teleport it to the new hold point
            if (currentItem != null && holdPoint != null)
            {
                currentItem.transform.SetParent(holdPoint);
                currentItem.transform.localPosition = Vector3.zero;
                currentItem.transform.localRotation = Quaternion.identity;
            }
            
            Debug.Log($"Player degraded to state {currentDegradationIndex}.");
        }
        else
        {
            Debug.Log("Player is at maximum degradation state.");
        }
    }

    /// <summary>
    /// Applies a specific degradation index, used by the save/load system.
    /// Deactivates all states, then activates the target one and reassigns references.
    /// </summary>
    public void ApplyDegradation(int targetIndex)
    {
        if (degradationStates == null || degradationStates.Count == 0) return;
        if (targetIndex < 0 || targetIndex >= degradationStates.Count) return;

        // Deactivate all state objects first
        for (int i = 0; i < degradationStates.Count; i++)
        {
            if (degradationStates[i].stateObject)
                degradationStates[i].stateObject.SetActive(false);
        }

        // Set the index and activate the correct state
        currentDegradationIndex = targetIndex;
        var state = degradationStates[currentDegradationIndex];

        if (state.stateObject) state.stateObject.SetActive(true);
        if (state.holdPoint) holdPoint = state.holdPoint;
        if (state.animator) _animator = state.animator;

        var movement = GetComponent<PlayerMovement>();
        if (movement)
        {
            movement.SetDegradationState(state.throwingPoint, state.animator);
        }

        Debug.Log($"Degradation loaded: applied state {currentDegradationIndex}.");
    }
}

[System.Serializable]
public struct PlayerDegradationState
{
    public GameObject stateObject;
    public Transform holdPoint;
    public Transform throwingPoint;
    public Animator animator;
}