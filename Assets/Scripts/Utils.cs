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
        yield return new WaitForSeconds(1f);

        var asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        if (asyncLoad != null) asyncLoad.completed += (asyncOperation) =>
        {
            GameManager.Instance.SetState(GameManager.GameState.Gameplay);
            if (loading) Object.Destroy(loading);
        };
    }
}