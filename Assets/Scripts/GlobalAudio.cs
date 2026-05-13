using System;
using UnityEngine;

public class GlobalAudio : MonoBehaviour
{
    private static GlobalAudio _instance;
    public static GlobalAudio Instance => _instance;
    
    private Camera _camera;

    private void Start()
    {
        _camera = Camera.current;
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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
