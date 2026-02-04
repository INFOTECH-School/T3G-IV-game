using UnityEngine;

public class LevelGate : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance.LevelOperator.canEndLevel1)
            {
                GameManager.Instance.LevelOperator.EndLevel(1);
            }
            Debug.Log("Player entered level end");
        }
    }
}

