
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    public List<GameObject> tutorialStepsObjects;

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
                tutorialStepsObjects[i].SetActive(i == (int)currentStep);
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
                // Handled by OnItemGrabbed()
                break;
            case TutorialStep.Aim:
                if (Input.GetMouseButtonDown(1))
                {
                    AdvanceTutorial();
                }
                break;
            case TutorialStep.Throw:
                if (Input.GetMouseButtonUp(1))
                {
                    AdvanceTutorial();
                }
                break;
            case TutorialStep.Jump:
                // Handled by OnJump()
                break;
            case TutorialStep.Interact:
                // Handled by OnInteractPressed()
                break;
            case TutorialStep.Push:
                // Handled by OnPushing()
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
            tutorialStepsObjects[(int)currentStep].SetActive(false);
        }

        currentStep++;
        hasAdvancedThisFrame = true;

        if (currentStep < TutorialStep.Done)
        {
            if ((int)currentStep < tutorialStepsObjects.Count && tutorialStepsObjects[(int)currentStep] != null)
            {
                tutorialStepsObjects[(int)currentStep].SetActive(true);
            }
        }
    }

    public void OnItemGrabbed()
    {
        if (currentStep == TutorialStep.Grab)
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
