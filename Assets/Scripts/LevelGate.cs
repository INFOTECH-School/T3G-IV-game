using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGate : MonoBehaviour
{
    public int level;
    public bool interact = false;
    public GameObject objectToEnable;
    private bool _inTrigger;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (interact)
            {
                _inTrigger = true;
            }
            else
            {
                HandleLevelEnd();
            }
            Debug.Log("Player entered level end");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (interact)
            {
                _inTrigger = false;
            }
        }
    }

    private void Update()
    {
        if (_inTrigger)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                HandleLevelEnd();
                _inTrigger = false;
            }
        }
    }

    private void HandleLevelEnd()
    {
        switch (level)
        {
            case 0: //Tutorial
                //SceneManager.LoadScene("Level1");   //Replace it with asynchronous scene loading.
                Utils.AsynchronousSceneLoad("Level1");
                break;
            case 1:
                if (GameManager.Instance.LevelOperator.canEndLevel1)
                {
                    GameManager.Instance.LevelOperator.EndLevel(1);
                }
                break;
            case 2:
                if (GameManager.Instance.LevelOperator.canEndLevel2)
                {
                    GameManager.Instance.LevelOperator.EndLevel(2);
                }
                break;
        }

        if (objectToEnable)
        {
            objectToEnable.SetActive(true);
        }
    }
}

