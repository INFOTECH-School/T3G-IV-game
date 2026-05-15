using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class Main_Menu : MonoBehaviour
{
    public List<Button> saveSlots = new List<Button>();
    public List<Button> deleteButtons = new List<Button>();
    [FormerlySerializedAs("LoadButton")] public Button loadGameButton;
    public Button newGameButton;
    public Color completedSaveColor = Color.green;
    public Color defaultSaveColor = new Color(1f, 0f, 0.4314132f);
    
    [Header("Other Credits")]
    public AssetCreditsViewer assetCreditsViewer;

    public void ShowOtherCredits()
    {
        if (assetCreditsViewer) assetCreditsViewer.ShowDocumentation();
    }
    public void Quit()
    {
        Application.Quit();
    }
    public void StartGame() //New Game
    {
        int nextSlot = 0;
        var saves = SaveManager.GetAllSaveSlots();
        if (saves.Count > 0)
        {
            var usedSlots = new HashSet<int>();
            foreach (var s in saves) usedSlots.Add(s.SlotNumber);
            while (usedSlots.Contains(nextSlot)) nextSlot++;
        }
        SetSaveSlot(nextSlot);
        Utils.AsynchronousSceneLoad("Scene_TutorialGym");
    }

    public void deleteButton(int slotNumber)
    {
        SaveManager.DeleteSave(slotNumber);
        UpdateSaveInfo();
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
        var saves = SaveManager.GetAllSaveSlots();
        int saveCount = saves.Count;
        Debug.Log($"Save count: {saveCount} and DisableSaveLoad: {Utils.DisableSaveLoad}");
        if (saveCount > 0 && !Utils.DisableSaveLoad)
        {
            loadGameButton.interactable = !Utils.DisableSaveLoad;
            newGameButton.interactable = saveCount < saveSlots.Count || saveSlots.Count == 0;
        }
        else
        {
            loadGameButton.interactable = false;
            newGameButton.interactable = true;
        }

        foreach (var saveSlot in saveSlots)
        {
            if (saveSlot != null) saveSlot.interactable = false;
        }
        foreach (var deleteBtn in deleteButtons)
        {
            if (deleteBtn != null)
            {
                deleteBtn.interactable = false;
                deleteBtn.GetComponent<Image>().color = defaultSaveColor;
            }
        }

        foreach (var save in saves)
        {
            bool isCompleted = SaveManager.IsSlotCompleted(save.SlotNumber);

            if (save.SlotNumber < saveSlots.Count && saveSlots[save.SlotNumber] != null)
            {
                saveSlots[save.SlotNumber].interactable = !isCompleted && !Utils.DisableSaveLoad;
            }
            if (save.SlotNumber < deleteButtons.Count && deleteButtons[save.SlotNumber] != null)
            {
                deleteButtons[save.SlotNumber].interactable = true;
                if (isCompleted)
                {
                    deleteButtons[save.SlotNumber].GetComponent<Image>().color = completedSaveColor;
                }
            }
        }
    }

    private void Start()
    {
        UpdateSaveInfo();
    }
}
