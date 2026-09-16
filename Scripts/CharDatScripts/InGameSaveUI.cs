using UnityEngine;

public class InGameSaveUI : MonoBehaviour
{
    public Character player;  // drag your Player object here

    public void SaveGame()
    {
        if (player == null)
        {
            Debug.LogError("InGameSaveUI: player reference not set.");
            return;
        }

        int slot = PlayerCreationData.CurrentSlot;

        var save = new PlayerSaveData
        {
            Name = player.char_Name,
            RaceId = player.char_Race,
            ClassId = player.char_Class,

            MaxHP = player.char_max_Health,
            Damage = player.char_Damage,
            INT = player.char_INT,
            SPD = player.char_SPD,
            LVL = player.char_Level,

            Luck = player.char_Luck,
            Charisma = player.char_Charisma,

            // If you store traits on Character, copy them here.
            // Traits = new List<string>(player.char_Traits)
            Traits = new System.Collections.Generic.List<string>(PlayerCreationData.Traits)
        };

        SaveSystem.SaveSlot(slot, save);
    }
}
