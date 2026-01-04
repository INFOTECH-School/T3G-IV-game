using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    private void Awake()
    {
        GameManager.Instance.RegisterPlayer(this);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance) GameManager.Instance.UnregisterPlayer();
    }

    void Start()
    {
        
    }
    
    void Update()
    {
        
    }
}
