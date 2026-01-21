using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class Main_Menu : MonoBehaviour
{
    public void Quit()
    {
        Application.Quit();
    }
    public void StartGame()
    {
        SceneManager.LoadScene("Scenes/Test_Gym");
    }   
}
