using UnityEngine;

public class BrokenWheel : MonoBehaviour
{
    public GameObject brokenWheelObject;
    public GameObject fixedWheelObject;
    public Item requiredItem; // The specific item needed to fix the wheel
    private LevelObjective _levelObjective;
    public GameObject objectToEnable;
    public GameObject sparklesToEnable;
    public bool truck;

    [Header("Guide Settings")]
    public GameObject guideGameObject;

    private void Start()
    {
        _levelObjective = GetComponent<LevelObjective>();
        brokenWheelObject.SetActive(true);
        fixedWheelObject.SetActive(false);
        if (guideGameObject)
        {
            guideGameObject.SetActive(false);
        }

        if (sparklesToEnable)
        {
            sparklesToEnable.SetActive(false);
        }
    }

    public void Fix()
    {
        brokenWheelObject.SetActive(false);
        fixedWheelObject.SetActive(true);
        if (_levelObjective)
        {
            _levelObjective.CompleteObjective();
        }

        if (truck && GameManager.Instance.LevelOperator.truckDependencyScore == 0)
        {
            if (sparklesToEnable)
            {
                sparklesToEnable.SetActive(true);
            }
        }
        if (objectToEnable)
        {
            objectToEnable.SetActive(true);
        }
        
        // Hide the guide and disable the script to prevent further interaction.
        if (guideGameObject)
        {
            guideGameObject.SetActive(false);
        }
        enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Show guide only if the wheel is not fixed and the player enters the trigger.
        if (other.CompareTag("Player") && guideGameObject != null && !fixedWheelObject.activeSelf)
        {
            guideGameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Hide guide when the player exits the trigger.
        if (other.CompareTag("Player") && guideGameObject != null)
        {
            guideGameObject.SetActive(false);
        }
    }
}
