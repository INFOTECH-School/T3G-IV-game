using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Utils //Player pos might not be getting loaded, level progres bar doesn't have progress after load
{
    public static bool IsCutsceneGhostModeActive = false;
    public static bool DisableSaveLoad = true; // Set to false to enable save/load functionality
    public static void AsynchronousSceneLoad(string sceneName, SaveData dataToLoad = null)
    {
        var loading = Object.Instantiate(Resources.Load<GameObject>("UI/LoadingScreen"));
        Object.DontDestroyOnLoad(loading);
        GameManager.Instance.SetState(GameManager.GameState.Loading);
        GameManager.Instance.StartCoroutine(LoadSceneAfterDelay(sceneName, loading, dataToLoad));
    }

    private static IEnumerator LoadSceneAfterDelay(string sceneName, GameObject loading, SaveData dataToLoad)
    {
        yield return new WaitForSeconds(0.5f);

        Application.backgroundLoadingPriority = ThreadPriority.Low;
        var asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        if (asyncLoad == null)
        {
            if(loading) Object.Destroy(loading);
            yield break;
        }

        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null; 
        }

        //yield return new WaitForSeconds(1.5f);

        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        yield return new WaitForEndOfFrame();
        
        if (dataToLoad != null)
        {
            GameManager.Instance.ApplyLoadedData(dataToLoad);
        }
        
        Application.backgroundLoadingPriority = ThreadPriority.Normal;
        
        yield return new WaitForSeconds(1.5f);
        if (loading) Object.Destroy(loading);

        InitializeIllustrationCutscenes();
        
        // Brute force check: if any active IllustrationCutscene is present, we must be in Cutscene state.
        if (GameManager.Instance.IsAnyCutsceneActive())
        {
            GameManager.Instance.SetState(GameManager.GameState.Cutscene);
        }
        else
        {
            GameManager.Instance.SetState(GameManager.GameState.Gameplay);
        }
        
        OnLoadingComplete?.Invoke();
    }

    public static event System.Action OnLoadingComplete;

    private static void InitializeIllustrationCutscenes()
    {
        IllustrationCutscene[] illustrationCutscenes = Object.FindObjectsByType<IllustrationCutscene>(FindObjectsSortMode.None);
        foreach (IllustrationCutscene cutscene in illustrationCutscenes)
        {
            cutscene.InitializeAfterLoad();
        }
    }

    public static void SetMainAudioMusic(AudioClip audioClip)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetMusic(audioClip);
        }
        else
        {
            Debug.LogWarning("GameManager instance not found. Cannot set music.");
        }
    }
    
    public static Item GetItemByID(string itemID)
    {
        if (string.IsNullOrEmpty(itemID)) return null;
        
        Item[] allItems = Object.FindObjectsByType<Item>(FindObjectsSortMode.None);

        foreach (Item item in allItems)
        {
            if (item.id == itemID)
            {
                return item;
            }
        }

        return null;
    }
    
    public static TimelineTrigger GetTimelineTriggerByName(string triggerName)
    {
        Debug.Log(triggerName + " trigger debug part 1");
        if (string.IsNullOrEmpty(triggerName)) return null;

        TimelineTrigger[] allTriggers = Object.FindObjectsByType<TimelineTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log(allTriggers + " trigger debug part 2");
        foreach (TimelineTrigger trigger in allTriggers)
        {
            Debug.Log(trigger.gameObject.name + " : " + triggerName + " trigger debug part 3");
            if (trigger.gameObject.name == triggerName)
            {
                return trigger;
            }
        }
        return null;
    }

    public static LevelObjective GetLevelObjectiveByID(string objectiveID)
    {
        if (string.IsNullOrEmpty(objectiveID)) return null;

        LevelObjective[] allObjectives = Object.FindObjectsByType<LevelObjective>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var objective in allObjectives)
        {
            if (objective.id == objectiveID)
            {
                return objective;
            }
        }

        return null;
    }

    public static List<ObjectiveData> GetLevelObjectivesData()
    {
        LevelObjective[] allObjectives = Object.FindObjectsByType<LevelObjective>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        List<ObjectiveData> objectivesData = new List<ObjectiveData>();

        foreach (LevelObjective objective in allObjectives)
        {
            ObjectiveData data = new ObjectiveData(
                objective.id,
                objective.isCompleted,
                new SerializableVector3(objective.transform.position),
                new SerializableQuaternion(objective.transform.rotation)
                );
            objectivesData.Add(data);
        }

        return objectivesData;
    }

    public static List<BasketData> GetBasketsProgressData()
    {
        Basket[] allBaskets = Object.FindObjectsByType<Basket>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        List<BasketData> basketsProgress = new List<BasketData>();

        foreach (Basket basket in allBaskets)
        {
            basketsProgress.Add(new BasketData(
                basket.gameObject.name, 
                basket.basketCounter, 
                (basket.holdingPoint1) && basket.holdingPoint1.activeSelf, 
                basket.holdingPoint2 && basket.holdingPoint2.activeSelf));
        }
  
        return basketsProgress;
    }
    
    public static Basket GetBasketByName(string basketName)
    {
        if (string.IsNullOrEmpty(basketName)) return null;

        Basket[] allBaskets = Object.FindObjectsByType<Basket>(FindObjectsSortMode.None);
        foreach (Basket basket in allBaskets)
        {
            if (basket.gameObject.name == basketName)
            {
                return basket;
            }
        }

        return null;
    }

    public static BrokenWheel GetBrokenWheelByName(string brokenWheelName)
    {
        if (string.IsNullOrEmpty(brokenWheelName)) return null;
        
        BrokenWheel[] allBrokenWheels = Object.FindObjectsByType<BrokenWheel>(FindObjectsSortMode.None);
        foreach (BrokenWheel brokenWheel in allBrokenWheels)
        {
            if (brokenWheel.gameObject.name == brokenWheelName)
            {
                return brokenWheel;
            }
        }
        return null;
    }

    public static List<BrokenWheelData> GetBrokenWheelProgressData()
    {
        BrokenWheel[] allBrokenWheels = Object.FindObjectsByType<BrokenWheel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        List<BrokenWheelData> brokenWheelsProgress = new List<BrokenWheelData>();

        foreach (BrokenWheel brokenWheel in allBrokenWheels)
        {
            brokenWheelsProgress.Add(new BrokenWheelData(brokenWheel.gameObject.name, brokenWheel.objectFixed));
        }

        return brokenWheelsProgress;
    }
    
    public static List<string> GetPlayedIllustrationCutscenes()
    {
        IllustrationCutscene[] illustrationCutscenes =
            Object.FindObjectsByType<IllustrationCutscene>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        List<string> playedIllustrationCutscenes = new List<string>();

        foreach (IllustrationCutscene cutscene in illustrationCutscenes)
        {
            if (cutscene.played)
            {
                playedIllustrationCutscenes.Add(cutscene.gameObject.name);
            }
        }
        return playedIllustrationCutscenes;
    }

    public static IllustrationCutscene GetIllustrationCutsceneByName(string illustrationCutsceneName)
    {
        if (string.IsNullOrEmpty(illustrationCutsceneName)) return null;

        IllustrationCutscene[] illustrationCutscenes =
            Object.FindObjectsByType<IllustrationCutscene>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (IllustrationCutscene cutscene in illustrationCutscenes)
        {
            if (cutscene.gameObject.name == illustrationCutsceneName)
            {
                return cutscene;
            }
        }
        return null;
    }
}
