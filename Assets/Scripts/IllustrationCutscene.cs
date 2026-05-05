using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IllustrationCutscene : MonoBehaviour
{
    [Tooltip("The list of illustrations to display.")]
    public List<Sprite> illustrations = new List<Sprite>();

    [Tooltip("The time each illustration is shown, in seconds. Default is 15s.")]
    public float showTime = 15f;

    [Tooltip("The UI Image component to display the illustrations on.")]
    public Image illustrationImage;

    [Tooltip("The CanvasGroup for the fade-to-black transition.")]
    public CanvasGroup fadeCanvasGroup;

    [Tooltip("The duration of the fade-in and fade-out effects.")]
    public float fadeDuration = 1f;

    [Tooltip("The GameObject to activate after the cutscene is finished.")]
    public GameObject objectToActivate;

    public GameObject infoText;
    
    private int _currentIndex;
    private Coroutine _cutsceneCoroutine;

    void Start()
    {
        // To start the cutscene automatically, call Illustrate().
        // To trigger it manually, call Illustrate() from another script.
        Illustrate();
    }

    /// <summary>
    /// Starts the illustration cutscene.
    /// </summary>
    public void Illustrate()
    {
        if (_cutsceneCoroutine != null) return;

        if (illustrations.Count > 0 && illustrationImage != null && fadeCanvasGroup != null)
        {
            gameObject.SetActive(true);
            illustrationImage.enabled = true;
            fadeCanvasGroup.gameObject.SetActive(true);
            _cutsceneCoroutine = StartCoroutine(PlayCutscene());
        }
        else
        {
            Debug.LogError("IllustrationCutscene is not set up correctly. Please assign all required fields in the Inspector.");
            gameObject.SetActive(false);
        }
    }

    private IEnumerator PlayCutscene()
    {
        GameManager.Instance.SetState(GameManager.GameState.Cutscene);
        if (infoText)
        {
            infoText.SetActive(true);
        }

        // Start with a black screen
        fadeCanvasGroup.alpha = 1f;
        _currentIndex = 0;

        while (_currentIndex < illustrations.Count)
        {
            // Set the new illustration while the screen is black
            illustrationImage.sprite = illustrations[_currentIndex];

            // Fade in (reveal the illustration)
            yield return StartCoroutine(Fade(0f)); // Fade to transparent

            // Wait for the timer or a spacebar press
            float timer = 0f;
            bool skipped = false;
            while (timer < showTime && !skipped)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    skipped = true;
                }
                timer += Time.deltaTime;
                yield return null;
            }

            // Fade out (back to black)
            yield return StartCoroutine(Fade(1f)); // Fade to black

            _currentIndex++;
        }

        // End of cutscene
        _cutsceneCoroutine = null;

        if (objectToActivate)
        {
            objectToActivate.SetActive(true);
        }

        // Disable all cutscene UI elements
        if (illustrationImage)
        {
            illustrationImage.enabled = false;
        }
        if (fadeCanvasGroup)
        {
            fadeCanvasGroup.gameObject.SetActive(false);
        }
        GameManager.Instance.SetState(GameManager.GameState.Gameplay);
        if (infoText)
        {
            infoText.SetActive(false);
        }

        gameObject.SetActive(false);
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeCanvasGroup.alpha;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = targetAlpha;
    }
}
