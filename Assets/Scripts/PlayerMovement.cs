using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.RegisterPlayerMovement(this);
    }
    
    private void OnDestroy()
    {
        if (GameManager.Instance) GameManager.Instance.UnregisterPlayerMovement();
    }

    void Update()
    {
        
    }
}
