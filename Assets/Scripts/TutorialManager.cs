
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    public List<GameObject> tutorialStepsObjects;
    public float fadeDuration = 0.5f;

    private enum TutorialStep
    {
        Walk,
        Run,
        Grab,
        Aim,
        Throw,
        Jump,
        Interact,
        Push,
        BlockCar,
        Place,
        Done
    }

    private TutorialStep currentStep = TutorialStep.Walk;

    private bool wPressed, aPressed, sPressed, dPressed;
    private bool hasAdvancedThisFrame = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        for (int i = 0; i < tutorialStepsObjects.Count; i++)
        {
            if (tutorialStepsObjects[i] != null)
            {
                bool isActive = (i == (int)currentStep);
                tutorialStepsObjects[i].SetActive(isActive);
                CanvasGroup cg = tutorialStepsObjects[i].GetComponent<CanvasGroup>();
                if (cg == null) cg = tutorialStepsObjects[i].AddComponent<CanvasGroup>();
                cg.alpha = isActive ? 1f : 0f;
            }
        }
    }

    void Update()
    {
        Debug.Log("Current Step: " + currentStep);
        if (currentStep == TutorialStep.Done) return;
    
        switch (currentStep)
        {
            case TutorialStep.Walk:
                if (Input.GetKeyDown(KeyCode.W)) wPressed = true;
                if (Input.GetKeyDown(KeyCode.A)) aPressed = true;
                if (Input.GetKeyDown(KeyCode.S)) sPressed = true;
                if (Input.GetKeyDown(KeyCode.D)) dPressed = true;
                int pressedCount = (wPressed ? 1 : 0) + (aPressed ? 1 : 0) + (sPressed ? 1 : 0) + (dPressed ? 1 : 0);
                if (pressedCount >= 2)
                {
                    AdvanceTutorial();
                }
                break;
            case TutorialStep.Run:
                if (Input.GetKey(KeyCode.LeftShift) && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)))
                {
                    AdvanceTutorial();
                }
                break;
            case TutorialStep.Grab:
                if (GameManager.Instance != null && GameManager.Instance.Player != null && GameManager.Instance.Player.currentItem != null)
                {
                    AdvanceTutorial();
                }
                break;
            case TutorialStep.Aim:
                if (Input.GetMouseButton(1))
                {
                    AdvanceTutorial();
                }
                break;
            case TutorialStep.Throw:
                // Handled by OnThrow()
                break;
            case TutorialStep.Jump:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    AdvanceTutorial();
                }
                // Also handled by OnJump() via PlayerMovement
                break;
            case TutorialStep.Interact:
                // Handled by OnInteractPressed()
                break;
            case TutorialStep.Push:
                if (GameManager.Instance != null && GameManager.Instance.Player != null)
                {
                    var interaction = GameManager.Instance.Player.GetComponent<PlayerInteraction>();
                    if (interaction != null && (interaction.currentState == Player.PlayerState.Pushing || interaction.currentState == Player.PlayerState.Interacting))
                    {
                        if (Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
                        {
                            AdvanceTutorial();
                        }
                    }
                }
                break;
            case TutorialStep.BlockCar:
                // Handled by OnCarBlocked()
                break;
            case TutorialStep.Place:
                // Handled by OnObjectPlaced()
                break;
        }
    }

    private void LateUpdate()
    {
        // Reset the flag at the end of the frame.
        hasAdvancedThisFrame = false;
    }

    private void AdvanceTutorial()
    {
        if (hasAdvancedThisFrame) return;

        if ((int)currentStep < tutorialStepsObjects.Count && tutorialStepsObjects[(int)currentStep] != null)
        {
            StartCoroutine(FadeCanvasGroup(tutorialStepsObjects[(int)currentStep], 0f, true));
        }

        currentStep++;
        hasAdvancedThisFrame = true;

        if (currentStep < TutorialStep.Done)
        {
            if ((int)currentStep < tutorialStepsObjects.Count && tutorialStepsObjects[(int)currentStep] != null)
            {
                GameObject nextObj = tutorialStepsObjects[(int)currentStep];
                CanvasGroup cg = nextObj.GetComponent<CanvasGroup>();
                if (cg == null) cg = nextObj.AddComponent<CanvasGroup>();
                
                if (!nextObj.activeSelf)
                {
                    cg.alpha = 0f;
                }
                
                StartCoroutine(FadeCanvasGroup(nextObj, 1f, false));
            }
        }
    }

    private IEnumerator FadeCanvasGroup(GameObject obj, float endAlpha, bool disableOnFinish)
    {
        if (obj == null) yield break;

        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();

        obj.SetActive(true);
        float startAlpha = cg.alpha;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            yield return null;
        }

        cg.alpha = endAlpha;
        if (disableOnFinish)
        {
            obj.SetActive(false);
        }
    }

    public void OnItemGrabbed()
    {
        if (currentStep == TutorialStep.Grab)
        {
            AdvanceTutorial();
        }
    }
    
    public void OnThrow()
    {
        if (currentStep == TutorialStep.Throw)
        {
            AdvanceTutorial();
        }
    }
    
    public void OnJump()
    {
        if (currentStep == TutorialStep.Jump)
        {
            AdvanceTutorial();
        }
    }

    public void OnInteractPressed()
    {
        if (currentStep == TutorialStep.Interact)
        {
            AdvanceTutorial();
        }
    }

    public void OnPushing()
    {
        if (currentStep == TutorialStep.Push)
        {
            AdvanceTutorial();
        }
    }

    public void OnCarBlocked()
    {
        if (currentStep == TutorialStep.BlockCar)
        {
            AdvanceTutorial();
        }
    }

    public void OnObjectPlaced()
    {
        if (currentStep == TutorialStep.Place)
        {
            AdvanceTutorial();
        }
    }
}
