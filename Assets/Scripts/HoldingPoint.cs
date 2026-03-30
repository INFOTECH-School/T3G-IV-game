using System;
using UnityEngine;

public class HoldingPoint : MonoBehaviour
{
    [SerializeField] private Transform holdingPoint1;
    [SerializeField] private Transform holdingPoint2;
    private bool _isHoldingPoint1Free;
    [SerializeField] private GameObject finishResult;

    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag("Holdable"))
    //     {
    //         if (_isHoldingPoint1Free)
    //         {
    //             holdingPoint1.gameObject.SetActive(true);
    //             _isHoldingPoint1Free = false;
    //             Destroy(other.gameObject);
    //         }
    //         else
    //         {
    //             
    //             holdingPoint2.gameObject.SetActive(true);
    //             finishResult.SetActive(true);
    //             Destroy(other.gameObject);
    //         }
    //     }
    // }
}
