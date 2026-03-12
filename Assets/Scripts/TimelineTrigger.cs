using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class TimelineTrigger : MonoBehaviour
{
    public PlayableDirector director;
    public bool playOnce = true;
    private bool _played;
    public bool ending = false;
    public bool level2 = false;
    [SerializeField] private Triggerer triggerer;
    private enum Triggerer
    {
        Player,
        Alice,
        Activable
    }
    public List<GameObject> hideObjects = new List<GameObject>();
    public List<GameObject> hideOutlines = new List<GameObject>();
    private Dictionary<GameObject, int> _outlineLayers = new Dictionary<GameObject, int>();

    public void Play()
    {
        if (playOnce && _played) return;
        if (!director)
        {
            Debug.LogWarning("TimelineTrigger: Director not set!", this);
            return;
        }
        PlayCutscene(SceneManager.GetActiveScene().name);
    }

    private void PlayCutscene(string sceneName)
    {
        if (ending)
        {
            switch (GameManager.Instance.LevelOperator.currentLevel)
            {
                case 1 when !GameManager.Instance.LevelOperator.canEndLevel1:
                case 2 when !GameManager.Instance.LevelOperator.canEndLevel2:
                    return;
            }
        }

        if (level2 && GameManager.Instance.LevelOperator.currentLevel != 2) return;
        GameManager.Instance.SetState(GameManager.GameState.Cutscene);
        foreach (var objectToHide in hideObjects)
        {
            if (objectToHide.activeSelf)
            {
                objectToHide.SetActive(false);
            }
        }

        foreach (var outlineToHide in hideOutlines)
        {
            if (outlineToHide)
            {
                _outlineLayers.Add(outlineToHide, outlineToHide.layer);
                outlineToHide.layer = LayerMask.NameToLayer("Hidden");
            }
        }

        director.stopped += OnCutsceneFinished;
        _played = true;
        director.Play();
        Debug.Log("Played" + _played + gameObject.name);
    }

    private void OnCutsceneFinished(PlayableDirector obj)
    {
        GameManager.Instance.SetState(GameManager.GameState.Gameplay);
        foreach (var objectToHide in hideObjects)
        {
            if (objectToHide)
            {
                objectToHide.SetActive(true);
            }
        }
        foreach (var outlineToHide in hideOutlines)
        {
            if (_outlineLayers.TryGetValue(outlineToHide, out var layer))
            {
                outlineToHide.layer = layer;
            }
        }
        director.stopped -= OnCutsceneFinished;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && triggerer == Triggerer.Player)
        {
            if (playOnce && _played) return;
            if (!director) return;
            PlayCutscene(SceneManager.GetActiveScene().name);
        }
        
        if (other.CompareTag("Alice") && triggerer == Triggerer.Alice)
        {
            if (playOnce && _played) return;
            if (!director) return;
            PlayCutscene(SceneManager.GetActiveScene().name);
        }

        if (other.CompareTag("Activable") && triggerer == Triggerer.Activable)
        {
            if (playOnce && _played) return;
            if (!director) return;
            PlayCutscene(SceneManager.GetActiveScene().name);
        }
    }
}
