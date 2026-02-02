using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelOperator : MonoBehaviour
{
    public bool canEndLevel1 = false;

    private int _level1DependencyScore;
    public int level1DependencyScore
    {
        get
        {
            return _level1DependencyScore;
        }
        set
        {
            if (value == 0)
            {
                canEndLevel1 = true;
            }
        }
    }
        
    private void Awake()
    {
        GameManager.Instance.RegisterLevelOperator(this);
    }

    private void OnDisable()
    {
        GameManager.Instance.UnregisterLevelOperator();
    }

    private void EndLevel(int number)
    {
        switch (number)
        {
            case 1:
                SceneManager.LoadSceneAsync("Level 1");
                canEndLevel1 = false;
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (canEndLevel1)
            {
                EndLevel(1);
            }
        }
    }
}
