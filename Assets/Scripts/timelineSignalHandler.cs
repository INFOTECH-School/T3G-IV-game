using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine.Playables;

public class timelineSignalHandler : MonoBehaviour
{
    [Header("Player Parenting")] public Transform playerParent;

    [Header("Kinematic Object Control")] public List<KinematicObject> kinematicObjectsToDisable;

    [Header("ShelfBoomSound")] public AudioClip shelfBoomSound;

    [Header("CameraShake")] public CinemachineImpulseSource shelfImpulseSource;
    [Header("CameraShake")] public CinemachineImpulseSource boomImpulseSource;

    [Header("AudioSettings")] public AudioSource mainAudioSource;

    public List<AudioClip> timelineMusics;

    public PlayableDirector cardboardDirector;

    /// <summary>
    // This function parents the Player to the specified 'playerParent' transform.
    // It's designed to be called from a Timeline signal.
    // The 'worldPositionStays' parameter is set to 'true' to ensure the player
    // maintains its current world position, rotation, and scale upon parenting,
    // preventing any sudden jumps or stretching. The player will then follow the parent's movements.
    /// </summary>
    public void ParentPlayer()
    {
        if (playerParent != null && GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            GameManager.Instance.Player.transform.SetParent(playerParent, true);
        }
        else
        {
            Debug.LogWarning("PlayerParent, GameManager, or Player not found. Cannot parent player.", this);
        }
    }

    /// <summary>
    // This function unparents the Player, returning it to the root of the scene.
    // It's designed to be called from a Timeline signal.
    // The 'worldPositionStays' parameter is set to 'true' to ensure the player
    // maintains its current world transform when it is unparented.
    /// </summary>
    public void UnparentPlayer()
    {
        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            GameManager.Instance.Player.transform.SetParent(null, true);
        }
        else
        {
            Debug.LogWarning("GameManager or Player not found. Cannot unparent player.", this);
        }
    }

    /// <summary>
    // This function iterates through the 'kinematicObjectsToDisable' list
    // and calls the DisableInteraction() method on each one.
    // It's designed to be called from a Timeline signal.
    /// </summary>
    public void DisableKinematicObjectInteractions()
    {
        if (kinematicObjectsToDisable != null)
        {
            foreach (var kinematicObject in kinematicObjectsToDisable)
            {
                if (kinematicObject != null)
                {
                    kinematicObject.DisableInteraction();
                }
                else
                {
                    Debug.LogWarning("Found a null entry in the kinematicObjectsToDisable list.", this);
                }
            }
        }
    }

    public void DroppingShelveSound()
    {

        if (shelfBoomSound)
        {
            AudioSource.PlayClipAtPoint(shelfBoomSound, Camera.current.transform.position);
        }
    }

    // This is the function the Signal will call
    public void TriggerShake(string type)
    {
        switch (type)
        {
            case "shelf":
                shelfImpulseSource.GenerateImpulse();
                break;
            case "boom":
                boomImpulseSource.GenerateImpulse();
                break;
        }
    }

    public void PlayTimelineMusic(int id)
    {
        mainAudioSource.clip = timelineMusics[id];
    }

    public void DisableDirector(string directorName)
    {
        switch (directorName)
        {
            case "cardboard1":
                cardboardDirector.enabled = false;
                break;
        }
    }

    public void DegradePlayer()
    {
        if (!Utils.IsCutsceneGhostModeActive)
        {
            GameManager.Instance.Player.Degrade();
        }
    }
}
