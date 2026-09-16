using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string SlotPath(int slot) =>
        Path.Combine(Application.persistentDataPath, $"player_save_slot_{slot}.json");

    public static bool SlotExists(int slot) => File.Exists(SlotPath(slot));

    public static void SaveSlot(int slot, PlayerSaveData data)
    {
        File.WriteAllText(SlotPath(slot), JsonUtility.ToJson(data, true));
        Debug.Log($"Saved slot {slot}: {SlotPath(slot)}");
    }

    public static bool TryLoadSlot(int slot, out PlayerSaveData data)
    {
        string path = SlotPath(slot);
        if (!File.Exists(path)) { data = null; return false; }

        data = JsonUtility.FromJson<PlayerSaveData>(File.ReadAllText(path));
        return data != null;
    }

    public static void DeleteSlot(int slot)
    {
        string path = SlotPath(slot);
        if (File.Exists(path)) File.Delete(path);
    }
    public static void SavePlayer(PlayerSaveData data)
    {
        int slot = PlayerCreationData.CurrentSlot;
        SaveSlot(slot, data);
    }
    public static bool TryLoadPlayer(out PlayerSaveData data)
    {
        int slot = PlayerCreationData.CurrentSlot;
        return TryLoadSlot(slot, out data);
    }


}
