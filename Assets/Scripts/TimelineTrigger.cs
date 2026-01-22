using System;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineTrigger : MonoBehaviour
{
    public PlayableDirector director;
    public bool playOnce = true;
    private bool _played;
    [SerializeField] private Triggerer triggerer;
    private enum Triggerer
    {
        Player,
        Alice
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && triggerer == Triggerer.Player)
        {
            if (playOnce && _played) return;
            if (!director) return;
            _played = true;
            director.Play();
            Debug.Log("Played");
        }
        
        if (other.CompareTag("Alice") && triggerer == Triggerer.Alice)
        {
            if (playOnce && _played) return;
            if (!director) return;
            _played = true;
            director.Play();
            Debug.Log("Played");
        }
    }
}
