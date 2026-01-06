using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.RegisterPlayer(this);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance) GameManager.Instance.UnregisterPlayer();
    }
    
    void Update()
    {
        
    }
}
