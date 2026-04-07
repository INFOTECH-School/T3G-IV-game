using System;
using UnityEngine;
using UnityEngine.Serialization;

public class LevelObjective : MonoBehaviour
{
    public bool truckObjective;
    public GameObject finishResult;
    [FormerlySerializedAs("_isCompleted")] public bool isCompleted = false;
    public string id;

    private void Start()
    {
        if (finishResult) finishResult.SetActive(false);
    }

    public void CompleteObjective()
    {
        if (isCompleted) return;

        isCompleted = true;
        if (truckObjective && GameManager.Instance.LevelOperator)
        {
            GameManager.Instance.LevelOperator.ProgressTruck();
        }

        if (finishResult) finishResult.SetActive(true);
        if (GameManager.Instance.LevelOperator)
        {
            GameManager.Instance.LevelOperator.ProgressLevel();
        }
    }

    public void RegressObjective()
    {
        if (!isCompleted) return;

        isCompleted = false;
        if (truckObjective && GameManager.Instance.LevelOperator)
        {
            GameManager.Instance.LevelOperator.RegressTruck();
        }

        if (finishResult) finishResult.SetActive(false);
        if (GameManager.Instance.LevelOperator)
        {
            GameManager.Instance.LevelOperator.RegressLevel();
        }
    }
}
