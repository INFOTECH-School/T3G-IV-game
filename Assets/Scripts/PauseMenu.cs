using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject PauseMenu;
    public GameObject settingsMenu;
    public GameObject ControlsPanel;
    public GameObject basePanel;
    
    private void Start()
    {
        PauseMenu.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("EScape pressed");
            ChangePauseMenuState();
            settingsMenu.SetActive(false);
            ControlsPanel.SetActive(false);
        }
    }

    public void ChangePauseMenuState()
    {
        if (PauseMenu)
        {
            if (PauseMenu.activeSelf)
            {
                basePanel.SetActive(true);
                settingsMenu.SetActive(false);
                ControlsPanel.SetActive(false);
                PauseMenu.SetActive(false);
                GameManager.Instance.SetState(GameManager.GameState.Gameplay);
            }
            else
            {
                basePanel.SetActive(true);
                settingsMenu.SetActive(false);
                ControlsPanel.SetActive(false);
                PauseMenu.SetActive(true);
                GameManager.Instance.SetState(GameManager.GameState.Paused);
            }
        }
    }
    
    public void QuitButton()
    {
        GameManager.Instance.SetState(GameManager.GameState.Gameplay);
        Utils.AsynchronousSceneLoad("MainMenu");
    }

    public void resumeButton()
    {
        ChangePauseMenuState();
    }

    public void saveButton()
    {
        // This will save the game to the currently selected save slot.
        // You'll need to set the CurrentSaveSlot in the GameManager when the player
        // selects a slot from your UI.
        GameManager.Instance.SaveGame(GameManager.Instance.CurrentSaveSlot);
    }

    // public void loadButton(int slotNumber)//main menu
    // {
    //     // This can be called from your UI to load a specific save slot.
    //     GameManager.Instance.LoadGame(slotNumber);
    // }
    //
    // public void SetSaveSlot(int slotNumber)
    // {
    //     // Call this from your UI when the player selects a save slot.
    //     GameManager.Instance.CurrentSaveSlot = slotNumber;
    // }
}