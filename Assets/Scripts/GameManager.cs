using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Player Player { private set; get; }
    public PlayerMovement PlayerMovement { private set; get; }
    public AliceController AliceController {private set; get;}
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (!Instance) Instance = this;
    }
    
    public void RegisterPlayer(Player player)
    {
        if (!player) return;
        Player = player;
    }
    
    public void UnregisterPlayer()
    {
        Player = null;
    }

    public void RegisterPlayerMovement(PlayerMovement playerMovement)
    {
        if (!playerMovement) return;
        PlayerMovement = playerMovement;
    }

    public void UnregisterPlayerMovement()
    {
        PlayerMovement = null;
    }

    public void RegisterAliceController(AliceController aliceController)
    {
        if (!aliceController) return;
        AliceController = aliceController;
    }
    
    public void UnregisterAliceController()
    {
        AliceController = null;
    }
    
}
