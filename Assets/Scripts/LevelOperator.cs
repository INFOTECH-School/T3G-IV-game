using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class LevelOperator : MonoBehaviour
{
    public List<string> destroyedItemsID = new List<string>();
    private List<string> _playedCutscenes = new List<string>();
    private List<ObjectiveData> _objectives = new List<ObjectiveData>();
    public int currentLevel = 1;
    public bool canEndLevel1 = false;
    public List<AudioClip> levelAudioClips = new List<AudioClip>();

    private int _level1DependencyScore = 6;
    private int _initialLevel1Score;
    public int level1DependencyScore
    {
        get { return _level1DependencyScore; }
        set
        {
            _level1DependencyScore = value;
            Debug.Log("level 1 set: " + value);

            if (_level1DependencyScore <= 0)
            {
                _level1DependencyScore = 0;
                canEndLevel1 = true;
                if (level1ProgressBarCanvas) level1ProgressBarCanvas.SetActive(false);
            }
            else
            {
                canEndLevel1 = false;
                if (level1ProgressBarCanvas) level1ProgressBarCanvas.SetActive(true);
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
            Debug.Log("level 2 set: " + value);
            if (_level2DependencyScore <= 0)
            {
                _level2DependencyScore = 0;
                canEndLevel2 = true;
                if (level2ProgressBarCanvas) level2ProgressBarCanvas.SetActive(false);
            }
            else
            {
                canEndLevel2 = false;
                if (level2ProgressBarCanvas) level2ProgressBarCanvas.SetActive(true);
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
        
        // Preload level audio clips to RAM
        foreach (var clip in levelAudioClips)
        {
            if (clip != null) clip.LoadAudioData();
        }
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

            if (levelAudioClips.Count >= currentLevel)
            {
                Utils.SetMainAudioMusic(levelAudioClips[currentLevel-1]);
            }
        }
        else if (number == 2)
        {
            if (level2ProgressBarCanvas) level2ProgressBarCanvas.SetActive(false);
        }
        //confettiParticles.Play();
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
        Utils.SetMainAudioMusic(levelAudioClips[currentLevel-1]);
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
        Utils.SetMainAudioMusic(levelAudioClips[currentLevel-1]);
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
            Debug.Log("Updating level 1 progress bar: " + _initialLevel1Score + ", " + _level1DependencyScore);
            level1ProgressBar.value = _initialLevel1Score - _level1DependencyScore;
        }
        else if (currentLevel == 2 && level2ProgressBar != null)
        {
            Debug.Log("Updating level 2 progress bar: " + _initialLevel2Score + ", " + _level2DependencyScore);
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

    public void AddPlayedCutscene(string cutsceneName)
    {
        _playedCutscenes.Add(cutsceneName);
    }

    public List<string> GetPlayedCutscenes()
    {
        return _playedCutscenes;
    }

    public List<ObjectiveData> GetObjectivesData()
    {
        _objectives = Utils.GetLevelObjectivesData();
        return _objectives;
    }
}
