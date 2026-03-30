using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class InteractableSpawner : MonoBehaviour
{
    [Header("The Setup")]
    [Tooltip("Tag of the object that can trigger this spawner (e.g., 'Throwable')")]
    public string targetTag = "Throwable";

    [Tooltip("The Timeline director for the cutscene to play.")]
    public PlayableDirector cutscene;

    [Tooltip("A list of GameObjects to spawn as rewards.")]
    public List<GameObject> itemsToSpawn;

    [Tooltip("The transform point where the rewards will be spawned.")]
    public Transform spawnPoint;

    [Tooltip("If set, this GameObject will be unparented to the root after the cutscene finishes.")]
    public GameObject objectToUnparent;

    public bool playOnce;

    private void OnEnable()
    {
        if (cutscene != null)
        {
            cutscene.stopped += OnCutsceneStopped;
        }
    }

    private void OnDisable()
    {
        if (cutscene != null)
        {
            cutscene.stopped -= OnCutsceneStopped;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(targetTag))
        {
            // Disable the throwable object to prevent re-triggering.
            other.gameObject.SetActive(false);
            
            GameManager.Instance.SetState(GameManager.GameState.Cutscene);
            if (cutscene != null)
            {
                cutscene.Play();
            }
        }
    }

    /// <summary>
    /// This method is called by a Timeline Signal Receiver to spawn the rewards.
    /// </summary>
    public void SpawnRewards()
    {
        if (itemsToSpawn == null || spawnPoint == null)
        {
            Debug.LogWarning("Items to spawn or spawn point is not set.", this);
            return;
        }

        foreach (GameObject item in itemsToSpawn)
        {
            if (item != null)
            {
                Instantiate(item, spawnPoint.position, spawnPoint.rotation);
            }
        }
    }

    /// <summary>
    /// This method is called when the cutscene has finished playing.
    /// </summary>
    /// <param name="director">The PlayableDirector that stopped.</param>
    private void OnCutsceneStopped(PlayableDirector director)
    {
        if (director == cutscene)
        {
            GameManager.Instance.SetState(GameManager.GameState.Gameplay);
            SpawnRewards();

            if (objectToUnparent != null)
            {
                objectToUnparent.transform.SetParent(null, true); // 'true' preserves world position, rotation, and scale
            }
        }

        if (playOnce)
        {
            Destroy(this);
        }
    }
}
