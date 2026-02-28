using UnityEngine;

public class LevelObjective : MonoBehaviour
{
    public void CompleteObjective()
    {
        GameManager.Instance.LevelOperator.ProgressLevel();
        // Optionally, disable this component to prevent repeated calls
        this.enabled = false; 
    }
}
