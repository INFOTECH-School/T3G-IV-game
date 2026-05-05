using System;
using UnityEngine;

public class Item : MonoBehaviour
{
    public string id;

    private void OnDestroy()
    {
        if (GameManager.Instance.LevelOperator) GameManager.Instance.LevelOperator.destroyedItemsID.Add(id);
    }
}
