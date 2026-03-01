using UnityEngine;

public class LevelObjective : MonoBehaviour
{
    public bool truckObjective;
    public void CompleteObjective()
    {
        if (truckObjective)
        {
            GameManager.Instance.LevelOperator.ProgressTruck();
        }
        GameManager.Instance.LevelOperator.ProgressLevel();
        // Optionally, disable this component to prevent repeated calls
        this.enabled = false;
    }
}
