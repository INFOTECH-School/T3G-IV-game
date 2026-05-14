using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject PauseMenu;
    public GameObject settingsMenu;
    public GameObject ControlsPanel;
    public GameObject basePanel;
    
    [Header("Save Button UI")]
    public Button saveBtn;
    public Image saveBtnImage;
    public Sprite defaultSaveSprite;
    public Sprite savedSprite;
    
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
                
                if (saveBtnImage && defaultSaveSprite)
                {
                    saveBtn.interactable = true;
                    saveBtnImage.overrideSprite = null;
                }

                saveBtn.interactable = SceneManager.GetActiveScene().name != "Scene_TutorialGym" && !Utils.DisableSaveLoad;
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
        if (Utils.DisableSaveLoad) return;
        // This will save the game to the currently selected save slot.
        // You'll need to set the CurrentSaveSlot in the GameManager when the player
        // selects a slot from your UI.
        GameManager.Instance.SaveGame(GameManager.Instance.CurrentSaveSlot);
        Debug.Log($"img: {saveBtnImage.name}, sprite: {savedSprite.name}");
        if (saveBtnImage && savedSprite)
        {
            // saveBtnImage.sprite = savedSprite;
            saveBtn.interactable = false;
            saveBtnImage.overrideSprite = savedSprite;
            Debug.Log($"Current sprite: {saveBtnImage.sprite.name}");
        }
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