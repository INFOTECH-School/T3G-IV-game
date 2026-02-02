using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class TimelineTrigger : MonoBehaviour
{
    public PlayableDirector director;
    public bool playOnce = true;
    private bool _played;
    [SerializeField] private Triggerer triggerer;
    private enum Triggerer
    {
        Player,
        Alice,
        Activable
    }

    private void PlayCutscene(string sceneName)
    {
        if (sceneName == "Level 1" && !GameManager.Instance.LevelOperator.canEndLevel1) return;
        _played = true;
        director.Play();
        Debug.Log("Played");
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
