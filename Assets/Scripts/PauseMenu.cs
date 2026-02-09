
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour

{
    public GameObject PauseMenu;

    private void Start()
    {
        PauseMenu.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OpenPauseMenu();
        }
    }
    public void OpenPauseMenu()
    {
        if (PauseMenu)
        {
            PauseMenu.SetActive(true);
            GameManager.Instance.CurrentGameState = GameManager.GameState.Paused;
        }
    }
    public void QuitButton()
    {
        GameManager.Instance.CurrentGameState = GameManager.GameState.Gameplay;
        SceneManager.LoadScene("MainMenu");
    }

    public void resumeButton()
    {
        PauseMenu.SetActive(false);
        GameManager.Instance.CurrentGameState = GameManager.GameState.Gameplay;
        
    }

    public void saveButton()
    {
        //SaveManager.Save()
        
    }
}