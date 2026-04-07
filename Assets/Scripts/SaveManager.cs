using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public static class SaveManager
{
    private const string SaveFilePrefix = "savegame_";
    private const string SaveFileExtension = ".json";

    private static string GetSavePath(int slotNumber)
    {
        return Path.Combine(Application.persistentDataPath, $"{SaveFilePrefix}{slotNumber}{SaveFileExtension}");
    }

    public static void SaveGame(int slotNumber, SaveData data)
    {
        string savePath = GetSavePath(slotNumber);
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
            Debug.Log($"Game saved successfully to {savePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game to slot {slotNumber}: {e.Message}");
        }
    }

    public static SaveData LoadGame(int slotNumber)
    {
        string savePath = GetSavePath(slotNumber);
        if (!File.Exists(savePath))
        {
            Debug.LogWarning($"No save file found for slot {slotNumber}.");
            return null;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            Debug.Log($"Game loaded successfully from {savePath}");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load game from slot {slotNumber}: {e.Message}");
            return null;
        }
    }

    public static void DeleteSave(int slotNumber)
    {
        string savePath = GetSavePath(slotNumber);
        if (File.Exists(savePath))
        {
            try
            {
                File.Delete(savePath);
                Debug.Log($"Save file for slot {slotNumber} deleted.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete save file for slot {slotNumber}: {e.Message}");
            }
        }
    }

    public static List<SaveSlotInfo> GetAllSaveSlots()
    {
        var saveSlots = new List<SaveSlotInfo>();
        var files = Directory.GetFiles(Application.persistentDataPath, $"{SaveFilePrefix}*{SaveFileExtension}");

        foreach (var file in files)
        {
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                int slotNumber = int.Parse(fileName.Replace(SaveFilePrefix, ""));
                saveSlots.Add(new SaveSlotInfo
                {
                    SlotNumber = slotNumber,
                    FilePath = file,
                    LastModified = File.GetLastWriteTime(file)
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not parse save file info for {file}: {e.Message}");
            }
        }
        return saveSlots.OrderBy(s => s.SlotNumber).ToList();
    }
}

public class SaveSlotInfo
{
    public int SlotNumber { get; set; }
    public string FilePath { get; set; }
    public DateTime LastModified { get; set; }
}