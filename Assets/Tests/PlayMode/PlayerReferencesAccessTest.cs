using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class PlayerReferencesAccessTest
{
    [UnityTest]
    public IEnumerator PlayerAccess()
    {
        SceneManager.LoadScene("Scenes/Test_Gym");
        yield return null;
        Assert.AreEqual(GameManager.Instance.Player, GameObject.FindGameObjectWithTag("Player").GetComponent<Player>());
    }
    
    [UnityTest]
    public IEnumerator PlayerMovementAccess()
    {
        SceneManager.LoadScene("Scenes/Test_Gym");
        yield return null;
        Assert.AreEqual(GameManager.Instance.PlayerMovement, GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>());
    }
}
