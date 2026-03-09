using UnityEngine;

public class BrokenWheel : MonoBehaviour
{
    public GameObject brokenWheelObject;
    public GameObject fixedWheelObject;
    public Item requiredItem; // The specific item needed to fix the wheel
    private LevelObjective _levelObjective;
    public GameObject objectToEnable;

    private void Start()
    {
        _levelObjective = GetComponent<LevelObjective>();
        brokenWheelObject.SetActive(true);
        fixedWheelObject.SetActive(false);
    }

    public void Fix()
    {
        brokenWheelObject.SetActive(false);
        fixedWheelObject.SetActive(true);
        if (_levelObjective)
        {
            _levelObjective.CompleteObjective();
        }

        if (objectToEnable)
        {
            objectToEnable.SetActive(true);
        }
        // The script is now disabled after fixing to prevent further interaction.
        enabled = false;
    }
}
