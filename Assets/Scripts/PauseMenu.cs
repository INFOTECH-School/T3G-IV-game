
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
            Time.timeScale = 0f;
        }
    }
    public void QuitButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void resumeButton()
    {
        Time.timeScale = 1f;
        PauseMenu.SetActive(false);
        
        
    }

    public void saveButton()
    {
        //SaveManager.Save()
        
    }
}