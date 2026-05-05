using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FallGuideTrigger : MonoBehaviour
{
    [SerializeField] private LevelObjective levelObjective;
    [SerializeField] private GameObject targetObject;

    private void Awake()
    {
        if (levelObjective == null)
        {
            Debug.LogError("FallGuideTrigger requires a LevelObjective component to be assigned.");
            enabled = false;
        }
        if (targetObject == null)
        {
            Debug.LogError("FallGuideTrigger requires a targetObject to be assigned.");
            enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == targetObject)
        {
            if (other.TryGetComponent<KinematicObject>(out var kinematicObject) && kinematicObject.movementType == KinematicObject.MovementType.Car)
            {
                return;
            }
            levelObjective.CompleteObjective();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == targetObject)
        {
            if (other.TryGetComponent<KinematicObject>(out var kinematicObject) && kinematicObject.movementType == KinematicObject.MovementType.Car)
            {
                return;
            }
            levelObjective.RegressObjective();
        }
    }
}
