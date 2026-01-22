using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class AliceControllerWaypointsTest
{
    
    [UnityTest]
    public IEnumerator AliceControllerAccess()
    {
        SceneManager.LoadScene("Scenes/Test_Gym");
        yield return null;
        Assert.AreEqual(GameManager.Instance.AliceController, GameObject.FindGameObjectWithTag("Alice").GetComponent<AliceController>());
    }
    
    [UnityTest]
    public IEnumerator AliceControllerWaypoints()
    {
        SceneManager.LoadScene("Scenes/Test_Gym");
        yield return null;
        Assert.AreEqual(GameManager.Instance.AliceController.currentTargetIndex, 0);
        yield return new WaitForSeconds(45);
        if (GameManager.Instance.AliceController.isLooped)
        { 
            Assert.AreEqual(GameManager.Instance.AliceController.currentTargetIndex, 0);   
        }
        else
        {
            Assert.AreNotEqual(GameManager.Instance.AliceController.currentTargetIndex, 0);
        }
    }
}
