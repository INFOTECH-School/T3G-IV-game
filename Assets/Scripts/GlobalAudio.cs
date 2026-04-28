using System;
using UnityEngine;

public class GlobalAudio : MonoBehaviour
{
    private Camera _camera;

    private void Start()
    {
        _camera = Camera.current;
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    
    void Update()
    {
        if (GameManager.Instance.Player)
        {
            this.transform.position = GameManager.Instance.Player.transform.position;
        } else if (_camera)
        {
            this.transform.position = _camera.transform.position;
        }
        else
        {
            if (Camera.current) this.transform.position = Camera.current.transform.position;
        }
    }
}
