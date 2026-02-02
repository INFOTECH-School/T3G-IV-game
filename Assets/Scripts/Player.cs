using UnityEngine;
using TMPro; // REQUIRED for TextMeshPro

public class Player : MonoBehaviour
{
    [Header("UI Settings")]
    public TextMeshProUGUI interactionText; // Drag your UI Text object here

    [Header("Settings")]
    public Transform holdPoint; 
    public LayerMask itemLayer;

    [Header("State")]
    public Item currentItem;
    
    // Track nearby objects
    private Item nearbyItem;
    private Basket nearbyBasket; 

    // Memories for the "Return to Shelf" feature
    private Vector3 originalPos;
    private Quaternion originalRot;

    public enum PlayerState { Normal, Pushing }
    private void Start()
    {
        if (GameManager.Instance) GameManager.Instance.RegisterPlayer(this);
        UpdateUIText(); // clear text on start
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
                if (nearbyBasket) PlaceInBasket();
                else Unequip();
            }
            else if (nearbyItem)
            {
                Equip(nearbyItem);
            }
            
            // Update UI immediately after any action
            UpdateUIText();
        }
    }

    // === ACTIONS ===

    public void Equip(Item itemToEquip)
    {
        if (!itemToEquip) return;

        currentItem = itemToEquip;
        originalPos = currentItem.transform.position;
        originalRot = currentItem.transform.rotation;

        var rb = currentItem.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        var col = currentItem.GetComponent<Collider>();
        if (col) col.enabled = false; 

        currentItem.transform.SetParent(holdPoint); 
        currentItem.transform.localPosition = Vector3.zero;
        currentItem.transform.localRotation = Quaternion.identity;
    }

    public void Unequip()
    {
        if (currentItem == null) return;

        currentItem.transform.SetParent(null);
        currentItem.transform.position = originalPos; 
        currentItem.transform.rotation = originalRot;

        ResetItemPhysics();
        currentItem = null;
    }

    public void PlaceInBasket()
    {
        if (currentItem == null || nearbyBasket == null) return;

        nearbyBasket.ReceiveItem(currentItem);
        currentItem = null;
    }

    private void ResetItemPhysics()
    {
        if (currentItem == null) return;
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
        
        UpdateUIText();
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
        if (basketScript && basketScript == nearbyBasket) nearbyBasket = null;
        
        UpdateUIText();
    }

    // === UI UPDATE ===
    private void UpdateUIText()
    {
        if (!interactionText) return;

        if (currentItem)
        {
            // We are holding something...
            if (nearbyBasket)
            {
                interactionText.text = "Press [E] to Place";
                interactionText.gameObject.SetActive(true);
            }
            else
            {
                // Optional: Show "Drop" text, or keep it hidden if you only want prompt for interactions
                interactionText.text = "Press [E] to Drop"; 
                interactionText.gameObject.SetActive(true);
            }
        }
        else
        {
            // Hands are empty...
            if (nearbyItem)
            {
                interactionText.text = "Press [E] to Pick Up " + nearbyItem.name;
                interactionText.gameObject.SetActive(true);
            }
            else
            {
                // Nothing nearby
                interactionText.gameObject.SetActive(false);
            }
        }
    }
}