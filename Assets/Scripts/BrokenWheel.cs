using UnityEngine;

public class BrokenWheel : MonoBehaviour
{
    public GameObject brokenWheelObject;
    public GameObject fixedWheelObject;
    public Item requiredItem; // The specific item needed to fix the wheel
    private LevelObjective _levelObjective;
    public GameObject objectToEnable;
    public GameObject sparklesToEnable;
    public bool truck;

    [Header("Guide Settings")]
    public GameObject guideGameObject;
    public float activationRange = 3f;

    private Transform _playerTransform;

    private void Start()
    {
        _levelObjective = GetComponent<LevelObjective>();
        brokenWheelObject.SetActive(true);
        fixedWheelObject.SetActive(false);
        if (guideGameObject)
        {
            guideGameObject.SetActive(false);
        }
        if (GameManager.Instance.Player != null)
        {
            _playerTransform = GameManager.Instance.Player.transform;
        }
    }

    private void Update()
    {
        if (_playerTransform != null && guideGameObject != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
            if (distanceToPlayer <= activationRange)
            {
                guideGameObject.SetActive(true);
            }
            else
            {
                guideGameObject.SetActive(false);
            }
        }
    }

    public void Fix()
    {
        brokenWheelObject.SetActive(false);
        fixedWheelObject.SetActive(true);
        if (_levelObjective)
        {
            _levelObjective.CompleteObjective();
        }

        if (truck && GameManager.Instance.LevelOperator.truckDependencyScore == 0)
        {
            if (sparklesToEnable)
            {
                sparklesToEnable.SetActive(true);
            }
        }
        if (objectToEnable)
        {
            objectToEnable.SetActive(true);
        }
        // The script is now disabled after fixing to prevent further interaction.
        enabled = false;
    }
}
