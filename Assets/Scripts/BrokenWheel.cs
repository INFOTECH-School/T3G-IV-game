using System;
using UnityEngine;

public class BrokenWheel : MonoBehaviour
{
    public GameObject brokenWheelObject;
    public GameObject fixedWheelObject;
    public Item itemObject;
    private bool _canFix;
    private LevelObjective _levelObjective;

    private void Start()
    {
        _levelObjective = GetComponent<LevelObjective>();
        brokenWheelObject.SetActive(true);
        fixedWheelObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance.Player.currentItem == itemObject)
            {
                Debug.Log("Can Fix");
                _canFix = true;
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _canFix = false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X) && _canFix)
        {
            Fix();
        }
    }

    private void Fix()
    {
        _canFix = false;
        brokenWheelObject.SetActive(false);
        fixedWheelObject.SetActive(true);
        if (_levelObjective)
        {
            _levelObjective.CompleteObjective();
        }
        GameManager.Instance.Player.currentItem = null;
        Destroy(itemObject.gameObject);
        enabled = false;
    }
}
