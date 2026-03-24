using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class Main_Menu : MonoBehaviour
{
    public void Quit()
    {
        Application.Quit();
    }
    public void StartGame() //New Game
    {
        Utils.AsynchronousSceneLoad("Scene_TutorialGym");
    }   
}
