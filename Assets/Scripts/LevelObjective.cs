using System;
using UnityEngine;

public class LevelObjective : MonoBehaviour
{
    public bool truckObjective;
    public GameObject finishResult;
    private bool _isCompleted = false;

    private void Start()
    {
        if (finishResult) finishResult.SetActive(false);
    }

    public void CompleteObjective()
    {
        if (_isCompleted) return;

        _isCompleted = true;
        if (truckObjective)
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
        if (!_isCompleted) return;

        _isCompleted = false;
        if (truckObjective)
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
