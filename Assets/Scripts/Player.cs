using UnityEngine;

public class Player : MonoBehaviour
{
    public enum PlayerState { Normal, Pushing, Interacting }
    private void Start()
    {
        GameManager.Instance.RegisterPlayer(this);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance) GameManager.Instance.UnregisterPlayer();
    }
}
