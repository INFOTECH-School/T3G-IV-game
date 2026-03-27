using System;
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
    
    // Track nearby objects
    private Item nearbyItem;
    private Basket nearbyBasket; 
    private BrokenWheel nearbyWheel;

    // Memories for the "Return to Shelf" feature
    private Vector3 originalPos;
    private Quaternion originalRot;
    private Vector3 originalScale;
    
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
}