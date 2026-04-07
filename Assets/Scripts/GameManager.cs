using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Player Player { private set; get; }
    public PlayerMovement PlayerMovement { private set; get; }
    public AliceController AliceController {private set; get;}
    public LevelOperator LevelOperator { private set; get;}
    public int CurrentSaveSlot { get; set; }

    public enum GameState
    {
        Gameplay,
        Paused,
        Cutscene
    }
    public GameState CurrentGameState = GameState.Gameplay;
    void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        } else if (Instance != this)
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public void SaveGame(int slotNumber)
    {
        if (Player == null || LevelOperator == null)
        {
            Debug.LogError("Cannot save game, critical components are missing!");
            return;
        }

        SaveData data = new SaveData
        {
            playerPosition = new SerializableVector3(Player.transform.position),
            playerState = Player.GetComponent<PlayerInteraction>().currentState,
            currentHeldItemId = Player.currentItem != null ? Player.currentItem.id : "",
            currentLevel = LevelOperator.currentLevel,
            level1DependencyScore = LevelOperator.level1DependencyScore,
            level2DependencyScore = LevelOperator.level2DependencyScore,
            truckDependencyScore = LevelOperator.truckDependencyScore,
            playedCutscenes = LevelOperator.GetPlayedCutscenes(),
            objectives = LevelOperator.GetObjectivesData(),
            levelProgress = 0,
            basketsProgress = Utils.GetBasketsProgressData(),
            brokenWheelsProgress = Utils.GetBrokenWheelProgressData(),
            destroyedItems = LevelOperator.destroyedItemsID
        };

        SaveManager.SaveGame(slotNumber, data);
    }

    public void LoadGame(int slotNumber)
    {
        SaveData data = SaveManager.LoadGame(slotNumber);
        if (data == null)
        {
            Debug.LogWarning("Load data not found, starting a new game.");
            return;
        }
        
        CurrentSaveSlot = slotNumber;
        // TODO: The scene name should probably come from save data in the future
        Utils.AsynchronousSceneLoad("Level1", data);
    }

    public void ApplyLoadedData(SaveData data)
    {
        if (data == null)
        {
            Debug.LogError("ApplyLoadedData was called with null data.");
            return;
        }

        if (Player)
        {
            Player.transform.position = data.playerPosition.ToVector3();
            Player.GetComponent<PlayerInteraction>().currentState = data.playerState;
            if (!string.IsNullOrEmpty(data.currentHeldItemId))
            {
                Player.Equip(Utils.GetItemByID(data.currentHeldItemId));
            }
        }

        if (data.destroyedItems.Count > 0)
        {
            foreach (var destroyedItemData in data.destroyedItems)
            {
                Item item = Utils.GetItemByID(destroyedItemData);
                if (item)
                {
                    item.gameObject.SetActive(false);
                    Destroy(item.gameObject);
                }
            }
        }
        
        if (data.playedCutscenes.Count > 0)
        {
            foreach (var cutscene in data.playedCutscenes)
            {
                var trigger = Utils.GetTimelineTriggerByName(cutscene);
                Debug.Log(trigger + " trigger debug part 4");
                if (trigger)
                {
                    trigger.GhostPlay(); // add fake or fast play to that func (without visualization).
                    LevelOperator.AddPlayedCutscene(cutscene);
                }
                Debug.Log("Cutscene " + cutscene + " has been played.");
            }
        }

        if (data.objectives.Count > 0)
        {
            foreach (var objective in data.objectives)
            {
                LevelObjective objectiveScript = Utils.GetLevelObjectiveByID(objective.id);
                if (!objectiveScript) continue;
                
                GameObject objectiveGameObject = objectiveScript.gameObject;
                if (!objectiveGameObject) continue;

                objectiveGameObject.transform.position = objective.position.ToVector3();
                objectiveGameObject.transform.rotation = objective.rotation.ToQuaternion();
                objectiveScript.isCompleted = objective.isCompleted;
            }
        }
        
        if (data.basketsProgress.Count > 0)
        {
            foreach (var basketData in data.basketsProgress)
            {
                Basket basket = Utils.GetBasketByName(basketData.basketName);
                if (basket)
                {
                    basket.basketCounter = basketData.basketCounter;
                    if (basket.holdingPoint1) basket.holdingPoint1.SetActive(basketData.holdingPoint1Active);
                    if (basket.holdingPoint2) basket.holdingPoint2.SetActive(basketData.holdingPoint2Active);
                }
            }
        }

        if (data.brokenWheelsProgress.Count > 0)
        {
            foreach (var brokenWheelData in data.brokenWheelsProgress)
            {
                BrokenWheel brokenWheel = Utils.GetBrokenWheelByName(brokenWheelData.brokenWheelName);
                if (brokenWheel)
                {
                    if (brokenWheelData.brokenWheelFixed)
                    {
                        brokenWheel.Fix();
                    }
                }
            }
        }

        
        if (LevelOperator)
        {
            LevelOperator.currentLevel = data.currentLevel;
            LevelOperator.level1DependencyScore = data.level1DependencyScore;
            LevelOperator.level2DependencyScore = data.level2DependencyScore;
            LevelOperator.truckDependencyScore = data.truckDependencyScore;
        }

        Debug.Log("Game state loaded.");
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
