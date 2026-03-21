using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour

{
    public GameObject PauseMenu;
    public GameObject settingsMenu;
    public GameObject ControlsPanel;

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
                PauseMenu.SetActive(false);
                GameManager.Instance.SetState(GameManager.GameState.Gameplay);
            }
            else
            {
                PauseMenu.SetActive(true);
                GameManager.Instance.SetState(GameManager.GameState.Paused);
            }
        }
    }
    
    
    public void QuitButton()
    {
        GameManager.Instance.SetState(GameManager.GameState.Gameplay);
        SceneManager.LoadScene("MainMenu");
    }

    public void resumeButton()
    {
        ChangePauseMenuState();
    }

    public void saveButton()
    {
        //SaveManager.Save()
        
    }
}