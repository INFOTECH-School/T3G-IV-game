using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Player Player { private set; get; }
    public PlayerMovement PlayerMovement { private set; get; }
    public AliceController AliceController {private set; get;}
    public LevelOperator LevelOperator { private set; get;}

    public enum GameState
    {
        Gameplay,
        Paused,
        Cutscene
    }
    public GameState CurrentGameState = GameState.Gameplay;
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (!Instance) Instance = this;
    }

    public void SetState(GameState state)
    {
        switch (state)
        {
            case GameState.Gameplay:
                Time.timeScale = 1;
                break;
            case GameState.Paused:
                Time.timeScale = 0;
                break;
            case GameState.Cutscene:
                Time.timeScale = 1;
                break;
        }
        CurrentGameState = state;
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

    public void RegisterLevelOperator(LevelOperator levelOperator)
    {
        if (!levelOperator) return;
        LevelOperator = levelOperator;
    }

    public void UnregisterLevelOperator()
    {
        LevelOperator = null;
    }
}
