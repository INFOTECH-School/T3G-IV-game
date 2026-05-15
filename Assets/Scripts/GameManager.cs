using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Player Player { private set; get; }
    public PlayerMovement PlayerMovement { private set; get; }
    public AliceController AliceController {private set; get;}
    public LevelOperator LevelOperator { private set; get;}
    public int CurrentSaveSlot { get; set; }
    public AudioSource mainAudioSource;
    private AudioSource secondaryAudioSource;
    private AudioSource _activeSource;
    private float _baseMusicVolume = 1.0f;
    private Coroutine _audioCoroutine;
    private AudioClip _tutorialBGClip;
    private List<IllustrationCutscene> _activeCutscenes = new List<IllustrationCutscene>();

    public enum GameState
    {
        Gameplay,
        Paused,
        Cutscene,
        Loading,
        ShortCutscene
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
    
    void Start()
    {
        if (mainAudioSource)
        {
            _baseMusicVolume = mainAudioSource.volume;
            mainAudioSource.ignoreListenerPause = true;
            _activeSource = mainAudioSource;
            
            // Create secondary source for crossfading
            secondaryAudioSource = gameObject.AddComponent<AudioSource>();
            secondaryAudioSource.playOnAwake = false;
            secondaryAudioSource.loop = true;
            secondaryAudioSource.ignoreListenerPause = true;
            secondaryAudioSource.spatialBlend = mainAudioSource.spatialBlend;
            secondaryAudioSource.outputAudioMixerGroup = mainAudioSource.outputAudioMixerGroup;
            secondaryAudioSource.volume = 0;
        }
        
        // Preload tutorial music to avoid lag later
        _tutorialBGClip = Resources.Load<AudioClip>("SFX/Story_Cutscenes/tutorialBG");
        if (_tutorialBGClip) _tutorialBGClip.LoadAudioData();
    }

    public void RegisterCutscene(IllustrationCutscene cutscene)
    {
        if (!_activeCutscenes.Contains(cutscene)) _activeCutscenes.Add(cutscene);
    }

    public void UnregisterCutscene(IllustrationCutscene cutscene)
    {
        if (_activeCutscenes.Contains(cutscene)) _activeCutscenes.Remove(cutscene);
    }

    public bool IsAnyCutsceneActive()
    {
        // Remove any null references (destroyed objects) and check count
        _activeCutscenes.RemoveAll(item => item == null || !item.gameObject.activeInHierarchy);
        return _activeCutscenes.Count > 0;
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
            destroyedItems = LevelOperator.destroyedItemsID,
            playedIllustrationCutscenes = Utils.GetPlayedIllustrationCutscenes(),
            degradationIndex = Player.currentDegradationIndex
        };

        SaveManager.SaveGame(slotNumber, data);
    }

    public void LoadGame(int slotNumber)
    {
        SetState(GameState.Loading);
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

        SetState(GameState.Loading);

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
                if (objectiveScript.isCompleted && objectiveScript.finishResult)
                {
                    objectiveScript.finishResult.SetActive(true);
                }
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
                    
                    if (basket.level2)
                    {
                        for (int i = 0; i < basket.basketCounter; i++)
                        {
                            if (i < basket.basketPoints.Count && basket.basketPoints[i] != null)
                            {
                                basket.basketPoints[i].SetActive(true);
                            }
                        }
                        if (basket.basketCounter >= basket.basketPoints.Count && basket.finishResult)
                        {
                            basket.finishResult.SetActive(true);
                        }
                    }

                    if (basket.holdingPoint1)
                    {
                        basket.holdingPoint1.SetActive(basketData.holdingPoint1Active);
                        basket.ChangeHoldingPointState(1, !basketData.holdingPoint1Active);
                    }

                    if (basket.holdingPoint2)
                    {
                        basket.holdingPoint2.SetActive(basketData.holdingPoint2Active);
                        basket.ChangeHoldingPointState(2, !basketData.holdingPoint2Active);
                    }

                    if (basket.level1 && basketData.holdingPoint1Active && basketData.holdingPoint2Active && basket.finishResult)
                    {
                        basket.finishResult.SetActive(true);
                    }
                    if (!basket.level1 && !basket.level2 && basketData.holdingPoint1Active && basket.finishResult)
                    {
                        basket.finishResult.SetActive(true);
                    }
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
            if (LevelOperator.truckTriggerCollider && LevelOperator.truckDependencyScore == 0)
            {
                LevelOperator.truckTriggerCollider.enabled = true;
            }
        }

        if (data.playedIllustrationCutscenes.Count > 0)
        {
            foreach (string illustrationCutsceneData in data.playedIllustrationCutscenes)
            {
                IllustrationCutscene illustrationCutscene =
                    Utils.GetIllustrationCutsceneByName(illustrationCutsceneData);
                if (illustrationCutscene)
                {
                    Debug.Log(illustrationCutscene + "set to true");
                    illustrationCutscene.played = true;
                    illustrationCutscene.GhostPlay();
                }
            }
        }
        
        if (Player)
        {
            Player.transform.position = data.playerPosition.ToVector3();
            Player.GetComponent<PlayerInteraction>().currentState = data.playerState;
            // Apply degradation state
            if (data.degradationIndex > 0)
            {
                Player.ApplyDegradation(data.degradationIndex);
            }

            if (!string.IsNullOrEmpty(data.currentHeldItemId))
            {
                Player.Equip(Utils.GetItemByID(data.currentHeldItemId));
            }
        }

        Debug.Log("Game state loaded.");
    }

    public void SetState(GameState state)
    {
        switch (state)
        {
            case GameState.Gameplay:
                Time.timeScale = 1;
                AudioListener.pause = false;
                StartVolumeFade(GetTargetVolume(state), 0.5f);
                
                if (SceneManager.GetActiveScene().name != "MainMenu" && SceneManager.GetActiveScene().name != "Scene_TutorialGym"){
                    if (_activeSource && !_activeSource.isPlaying)
                    {
                        _activeSource.Play();
                    }
                } else if (SceneManager.GetActiveScene().name == "Scene_TutorialGym")
                {
                    if (_tutorialBGClip) SetMusic(_tutorialBGClip);
                    Debug.Log(SceneManager.GetActiveScene().name + " set audio");
                }
                break;
            case GameState.ShortCutscene:
                if (CurrentGameState == GameState.Loading) return;
                Time.timeScale = 0;
                break;
            case GameState.Paused:
                Time.timeScale = 0;
                AudioListener.pause = true;
                if (_activeSource)
                {
                    StartVolumeFade(GetTargetVolume(state), 0.5f);
                    if (!_activeSource.isPlaying) _activeSource.Play();
                }
                break;
            case GameState.Cutscene:
                Time.timeScale = 1;
                AudioListener.pause = false;
                if (_activeSource)
                {
                    StartVolumeFade(GetTargetVolume(state), 0.5f);
                    if (!_activeSource.isPlaying) _activeSource.Play();
                }
                break;
            case GameState.Loading:
                AudioListener.pause = false;
                if (_activeSource && _activeSource.isPlaying)
                {
                    _activeSource.Stop();
                }
                if (secondaryAudioSource && secondaryAudioSource.isPlaying)
                {
                    secondaryAudioSource.Stop();
                }
                Time.timeScale = 1;
                break;
                
        }
        CurrentGameState = state;
    }

    public void SetMusic(AudioClip clip)
    {
        if (!_activeSource || !clip) return;
        
        // If same clip is already playing, just ensure we are fading to correct volume
        if (_activeSource.clip == clip && _activeSource.isPlaying)
        {
            StartVolumeFade(GetTargetVolume(CurrentGameState), 0.5f);
            return;
        }

        // Trigger asynchronous load of audio data if not ready
        if (clip.loadState == AudioDataLoadState.Unloaded) clip.LoadAudioData();

        if (_audioCoroutine != null) StopCoroutine(_audioCoroutine);
        _audioCoroutine = StartCoroutine(CrossfadeCoroutine(clip, 1.0f));
    }

    private void StartVolumeFade(float targetVolume, float duration)
    {
        if (!_activeSource) return;
        if (_audioCoroutine != null) StopCoroutine(_audioCoroutine);
        _audioCoroutine = StartCoroutine(FadeVolumeCoroutine(targetVolume, duration));
    }

    private System.Collections.IEnumerator FadeVolumeCoroutine(float targetVolume, float duration)
    {
        float startVolume = _activeSource.volume;
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _activeSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }
        _activeSource.volume = targetVolume;
        _audioCoroutine = null;
    }

    private System.Collections.IEnumerator CrossfadeCoroutine(AudioClip newClip, float duration)
    {
        AudioSource oldSource = _activeSource;
        AudioSource newSource = (_activeSource == mainAudioSource) ? secondaryAudioSource : mainAudioSource;
        
        newSource.clip = newClip;
        newSource.volume = 0;
        newSource.Play();
        
        float oldStartVolume = oldSource.volume;
        float targetVolume = GetTargetVolume(CurrentGameState);
        float elapsed = 0;
        
        _activeSource = newSource; // New source is now the authority

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            
            oldSource.volume = Mathf.Lerp(oldStartVolume, 0, t);
            newSource.volume = Mathf.Lerp(0, targetVolume, t);
            
            yield return null;
        }
        
        oldSource.Stop();
        oldSource.volume = 0;
        newSource.volume = targetVolume;
        _audioCoroutine = null;
    }

    private float GetTargetVolume(GameState state)
    {
        switch (state)
        {
            case GameState.Gameplay:
                return 0.15f;
            case GameState.Paused:
                return 0.15f * 0.75f;
            case GameState.Cutscene:
                return 0.15f * 0.5f;
            default:
                return 0.15f;
        }
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

//TO DO: ensure all outlines are disabled during timelines. Adjust game sounds (land lower) all game higher, balance it out.