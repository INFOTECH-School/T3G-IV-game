using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelOperator : MonoBehaviour
{
    public int currentLevel = 1;
    public bool canEndLevel1 = false;

    private int _level1DependencyScore = 6;
    private int _initialLevel1Score;
    public int level1DependencyScore
    {
        get { return _level1DependencyScore; }
        set
        {
            _level1DependencyScore = value;
            if (_level1DependencyScore <= 0)
            {
                _level1DependencyScore = 0;
                canEndLevel1 = true;
                if (level1ProgressBarCanvas) level1ProgressBarCanvas.SetActive(false);
            }
            UpdateProgressBar();
        }
    }

    public bool canEndLevel2 = false;
    private int _level2DependencyScore = 9;
    private int _initialLevel2Score;

    public int level2DependencyScore
    {
        get { return _level2DependencyScore; }
        set
        {
            _level2DependencyScore = value;
            if (_level2DependencyScore <= 0)
            {
                _level2DependencyScore = 0;
                canEndLevel2 = true;
                if (level2ProgressBarCanvas) level2ProgressBarCanvas.SetActive(false);
            }
            UpdateProgressBar();
        }
    }

    [Header("UI Settings")]
    public Slider level1ProgressBar;
    public GameObject level1ProgressBarCanvas;
    public Slider level2ProgressBar;
    public GameObject level2ProgressBarCanvas;
    public ParticleSystem confettiParticles;

    [Header("Truck Settings")] 
    public int truckDependencyScore = 3;
    public BoxCollider truckTriggerCollider;

    private void Awake()
    {
        _initialLevel1Score = _level1DependencyScore;
        _initialLevel2Score = _level2DependencyScore;
    }

    private void Start()
    {
        if (GameManager.Instance) GameManager.Instance.RegisterLevelOperator(this);
        if (truckTriggerCollider)
        {
            truckTriggerCollider.enabled = false;
        }

        // Configure Level 1 UI
        if (level1ProgressBar != null)
        {
            level1ProgressBar.maxValue = _initialLevel1Score;
            level1ProgressBarCanvas.SetActive(true);
        }

        // Configure and hide Level 2 UI
        if (level2ProgressBar != null)
        {
            level2ProgressBar.maxValue = _initialLevel2Score;
            level2ProgressBarCanvas.SetActive(false);
        }
        
        UpdateProgressBar();
    }

    private void OnDisable()
    {
        if (GameManager.Instance) GameManager.Instance.UnregisterLevelOperator();
    }

    public void EndLevel(int number)
    {
        if (number == 1)
        {
            Debug.Log("Ending level 1");
            canEndLevel1 = false;
            currentLevel = 2;

            // Switch UI
            if (level1ProgressBarCanvas) level1ProgressBarCanvas.SetActive(false);
            if (level2ProgressBarCanvas) level2ProgressBarCanvas.SetActive(true);
            
            UpdateProgressBar(); // Update the new progress bar to its initial state
        }
        else if (number == 2)
        {
            if (level2ProgressBarCanvas) level2ProgressBarCanvas.SetActive(false);
        }
        confettiParticles.Play();
    }

    public void ProgressLevel()
    {
        switch (currentLevel)
        {
            case 1:
                level1DependencyScore--;
                Debug.Log("Level score:" + level1DependencyScore);
                break;
            case 2:
                level2DependencyScore--;
                Debug.Log("Level score:" + level2DependencyScore);
                break;
        }
    }

    public void RegressLevel()
    {
        switch (currentLevel)
        {
            case 1:
                level1DependencyScore++;
                Debug.Log("Level score:" + level1DependencyScore);
                break;
            case 2:
                level2DependencyScore++;
                Debug.Log("Level score:" + level2DependencyScore);
                break;
        }
    }

    public void ProgressTruck()
    {
        if (truckDependencyScore > 0)
        {
            truckDependencyScore--;
        }

        if (truckTriggerCollider && truckDependencyScore == 0)
        {
            truckTriggerCollider.enabled = true;
        }
    }

    private void UpdateProgressBar()
    {
        if (currentLevel == 1 && level1ProgressBar != null)
        {
            level1ProgressBar.value = _initialLevel1Score - _level1DependencyScore;
        }
        else if (currentLevel == 2 && level2ProgressBar != null)
        {
            level2ProgressBar.value = _initialLevel2Score - _level2DependencyScore;
        }
    }

    public void RegressTruck()
    {
        truckDependencyScore++;
        if (truckTriggerCollider)
        {
            truckTriggerCollider.enabled = false;
        }
    }
}
