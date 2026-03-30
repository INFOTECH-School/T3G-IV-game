using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Utils
{
    public static void AsynchronousSceneLoad(string sceneName)
    {
        var loading = Object.Instantiate(Resources.Load<GameObject>("UI/LoadingScreen"));
        Object.DontDestroyOnLoad(loading);
        GameManager.Instance.SetState(GameManager.GameState.Cutscene);
        GameManager.Instance.StartCoroutine(LoadSceneAfterDelay(sceneName, loading));
    }

    private static IEnumerator LoadSceneAfterDelay(string sceneName, GameObject loading)
    {
        yield return new WaitForSeconds(0.5f);

        Application.backgroundLoadingPriority = ThreadPriority.Low;
        var asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        // TWÓJ ORYGINALNY KOD - podpinamy event od razu, tak jak to miałeś na początku
        if (asyncLoad != null) asyncLoad.completed += (asyncOperation) =>
        {
            Application.backgroundLoadingPriority = ThreadPriority.Normal;
            GameManager.Instance.SetState(GameManager.GameState.Gameplay);
            if (loading) Object.Destroy(loading);
        };

        // Zatrzymujemy Unity przed automatycznym wejściem do sceny
        asyncLoad.allowSceneActivation = false;

        // Czekamy, aż Unity załaduje tło (zatrzyma się na 90%)
        while (asyncLoad.progress < 0.9f)
        {
            yield return null; 
        }

        // --- Czas na płynną animację (możesz to wydłużyć/skrócić) ---
        yield return new WaitForSeconds(1.5f);

        // Pozwalamy scenie się aktywować. Kiedy to się skończy, 
        // Unity automatycznie wywoła Twój event "completed" i zniszczy ekran.
        asyncLoad.allowSceneActivation = true;
    }
}