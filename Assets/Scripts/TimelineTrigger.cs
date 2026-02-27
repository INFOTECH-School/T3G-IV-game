using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class TimelineTrigger : MonoBehaviour
{
    public PlayableDirector director;
    public bool playOnce = true;
    private bool _played;
    public bool ending = false;
    [SerializeField] private Triggerer triggerer;
    private enum Triggerer
    {
        Player,
        Alice,
        Activable
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

        _played = true;
        director.Play();
        Debug.Log("Played" + _played + gameObject.name);
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
