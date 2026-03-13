using System;
using UnityEngine;

public class LevelObjective : MonoBehaviour
{
    public bool truckObjective;
    public GameObject finishResult;

    private void Start()
    {
        if (finishResult) finishResult.SetActive(false);
    }

    public void CompleteObjective()
    {
        if (truckObjective)
        {
            GameManager.Instance.LevelOperator.ProgressTruck();
        }

        if (finishResult) finishResult.SetActive(true);
        if (GameManager.Instance.LevelOperator)
        {
            GameManager.Instance.LevelOperator.ProgressLevel();
        }

        // Optionally, disable this component to prevent repeated calls
        this.enabled = false;
    }
}
