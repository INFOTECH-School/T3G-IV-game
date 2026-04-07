using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class Main_Menu : MonoBehaviour
{
    public List<Button> saveSlots = new List<Button>();
    [FormerlySerializedAs("LoadButton")] public Button loadGameButton;
    public Button newGameButton;
    public void Quit()
    {
        Application.Quit();
    }
    public void StartGame() //New Game
    {
        SetSaveSlot(SaveManager.GetAllSaveSlots().Count);
        Utils.AsynchronousSceneLoad("Scene_TutorialGym");
    }
    
    public void loadButton(int slotNumber)//main menu
    {
        // This can be called from your UI to load a specific save slot.
        GameManager.Instance.LoadGame(slotNumber);
    }

    public void SetSaveSlot(int slotNumber)
    {
        // Call this from your UI when the player selects a save slot.
        GameManager.Instance.CurrentSaveSlot = slotNumber;
    }

    public void UpdateSaveInfo()
    {
        if (SaveManager.GetAllSaveSlots().Count > 0)
        {
            loadGameButton.interactable = true;
            newGameButton.interactable = true;
            foreach (var saveSlot in saveSlots)
            {
                saveSlot.interactable = false;
            }

            foreach (var saveSlot in SaveManager.GetAllSaveSlots())
            {
                if (saveSlots[saveSlot.SlotNumber])
                {
                    saveSlots[saveSlot.SlotNumber].interactable = true;
                }
            }
        } else if (SaveManager.GetAllSaveSlots().Count >= saveSlots.Count)
        {
            loadGameButton.interactable = true;
            newGameButton.interactable = false;
        }
        else
        {
            loadGameButton.interactable = false;
            newGameButton.interactable = true;
        }
    }

    private void Start()
    {
        UpdateSaveInfo();
    }
}
