using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelOperator : MonoBehaviour
{
    public int currentLevel = 1;
    public bool canEndLevel1 = false;

    private int _level1DependencyScore = 6;
    public int level1DependencyScore
    {
        get
        {
            return _level1DependencyScore;
        }
        set
        {
            if (value <= 0)
            {
                canEndLevel1 = true;
                _level1DependencyScore = 0;
            }
            else
            {
                _level1DependencyScore = value;
            }
        }
    }
    public bool canEndLevel2 = false;
    private int _level2DependencyScore = 2;

    public int level2DependencyScore
    {
        get
        {
            return _level2DependencyScore;
        }
        set
        {
            if (value <= 0)
            {
                canEndLevel2 = true;
            }
            else
            {
                _level2DependencyScore = value;
            }
        }
    }
        
    private void Awake()
    {
        GameManager.Instance.RegisterLevelOperator(this);
        
        //set Level Dependency scores;
    }

    private void OnDisable()
    {
        GameManager.Instance.UnregisterLevelOperator();
    }

    public void EndLevel(int number)
    {
        switch (number)
        {
            case 1:
                //SceneManager.LoadSceneAsync("Test_Gym"/*"Level2"*/);
                Debug.Log("Ending level 1");
                canEndLevel1 = false;
                break;
        }
        currentLevel++;
    }

    public void ProgressLevel()
    {
        switch (currentLevel)
        {
            case 1:
                level1DependencyScore--;
                break;
            case 2:
                level2DependencyScore--;
                break;
        }
        Debug.Log("Level score:" + level1DependencyScore);
    }
}

