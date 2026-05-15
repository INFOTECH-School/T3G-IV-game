using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class IllustrationCutscene : MonoBehaviour
{
    [Tooltip("The PlayableDirector that plays the timeline cutscene.")]
    public PlayableDirector director;

    [Tooltip("The GameObject to activate after the cutscene is finished.")]
    public GameObject objectToActivate;

    public GameObject infoText;
    public GameObject IllustrationCanvas;
    
    private Coroutine _cutsceneCoroutine;
    public bool played;

    public void InitializeAfterLoad()
    {
        // To start the cutscene automatically, call Illustrate().
        // To trigger it manually, call Illustrate() from another script.
        if (!played)
        {
            Illustrate();
        }
    }

    public void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterCutscene(this);
            if (GameManager.Instance.CurrentGameState != GameManager.GameState.Loading && !played)
            {
                Illustrate();
            }
        }
        director = GetComponent<PlayableDirector>();
    }

    void OnDisable()
    {
        if (GameManager.Instance != null) GameManager.Instance.UnregisterCutscene(this);
    }

    /// <summary>
    /// Starts the illustration cutscene.
    /// </summary>
    public void Illustrate()
    {
        Debug.Log("Started Illustrating: " + played + " " + GameManager.Instance.CurrentGameState.ToString());
        
        if (_cutsceneCoroutine != null) return;
        played = true;
        
        if (director)
        {
            IllustrationCanvas.SetActive(true);
            gameObject.SetActive(true);
            _cutsceneCoroutine = StartCoroutine(PlayCutscene());
        }
        else
        {
            Debug.LogError("IllustrationCutscene is missing a PlayableDirector. Please assign it in the Inspector.");
            gameObject.SetActive(false);
        }
    }

    private IEnumerator PlayCutscene()
    {
        GameManager.Instance.SetState(GameManager.GameState.Cutscene);
        if (infoText)
        {
            infoText.SetActive(true);
        }

        // Start the timeline
        director.Play();

        // Give the director a frame to start playing
        yield return null;

        bool skipped = false;

        // Wait for the timeline to finish or for skip input
        while (director.state == PlayState.Playing && !skipped)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                skipped = true;
            }
            yield return null;
        }

        // If skipped, fast forward timeline to the end to ensure final state
        if (skipped)
        {
            if (director.playableAsset != null)
            {
                director.time = director.playableAsset.duration;
                director.Evaluate();
            }
            director.Stop();
        }

        EndCutscene();
    }

    private void EndCutscene()
    {
        _cutsceneCoroutine = null;

        if (objectToActivate)
        {
            objectToActivate.SetActive(true);
        }

        GameManager.Instance.SetState(GameManager.GameState.Gameplay);
        
        if (infoText)
        {
            infoText.SetActive(false);
        }

        IllustrationCanvas.SetActive(false);    
        gameObject.SetActive(false);
    }

    public void GhostPlay(){
        IllustrationCanvas.SetActive(false);
        gameObject.SetActive(false);
    }
}
