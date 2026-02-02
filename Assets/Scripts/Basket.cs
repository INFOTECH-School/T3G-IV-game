using UnityEngine;

public class Basket : MonoBehaviour
{
    [Header("Visual References")]
    // Drag the object you want to appear in the first slot (disable it in Editor first)
    public GameObject holdingPoint1; 
    
    // Drag the object you want to appear in the second slot (disable it in Editor first)
    public GameObject holdingPoint2; 
    
    // The object/effect that appears when both are full
    public GameObject finishResult; 

    // Internal tracking
    private bool _isHoldingPoint1Free = true;
    private bool _isHoldingPoint2Free = true;

    private void Start()
    {
        // Safety check: Ensure points are hidden when game starts
        if (holdingPoint1) holdingPoint1.SetActive(false);
        if (holdingPoint2) holdingPoint2.SetActive(false);
        if (finishResult) finishResult.SetActive(false);
    }

    // Called by Player when they press 'E'
    public void ReceiveItem(Item item)
    {
        if (_isHoldingPoint1Free)
        {
            // 1. Activate the visual representation in the basket
            if (holdingPoint1) holdingPoint1.SetActive(true);
            
            // 2. Mark slot as taken
            _isHoldingPoint1Free = false;

            // 3. DESTROY the item the player was holding
            Destroy(item.gameObject);
        }
        else if (_isHoldingPoint2Free)
        {
            // 1. Activate the second visual representation
            if (holdingPoint2) holdingPoint2.SetActive(true);
            
            // 2. Mark slot as taken
            _isHoldingPoint2Free = false;

            // 3. DESTROY the item the player was holding
            Destroy(item.gameObject);

            // 4. Trigger the result (Basket is now full)
            Debug.Log("Basket Full! Sequence Complete.");
            if (finishResult) finishResult.SetActive(true);
        }
    }

    // Helper for the Player script to know if it should show the "Press E" text
    public bool HasSpace()
    {
        return _isHoldingPoint1Free || _isHoldingPoint2Free;
    }
}