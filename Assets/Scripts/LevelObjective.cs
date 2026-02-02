using System;
using System.Collections;
using UnityEngine;

public class LevelObjective : MonoBehaviour
{
    public Transform destination;

    private void Start()
    {
        StartCoroutine(MovementCheck());
    }

    IEnumerator MovementCheck()
    {
        while (this.enabled)
        {
            // We only check 10 times a second instead of 60+
            if (ReachedDestination())
            {
                GameManager.Instance.LevelOperator.level1DependencyScore -= 1;
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private bool ReachedDestination()
    {
        float dist = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(destination.position.x, 0, destination.position.z)
        );
        return dist < 0.1f;
    }
}
