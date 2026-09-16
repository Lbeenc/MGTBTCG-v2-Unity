using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("UI")]
    public UIController uiController;

    [Header("Identity")]
    public string char_Name;
    public string char_Race;
    public string char_Class;

    [Header("Combat Stats")]
    public int char_max_Health;
    public int char_current_Health;
    public int char_Damage;
    public int char_INT;
    public int char_SPD;

    [Header("Hidden Stats")]
    public int char_Luck;
    public int char_Charisma;

    [Header("Progression")]
    public int char_Level = 1;
    public int char_Gold = 0;
    public int char_current_XP = 0;
    public int char_max_XP = 10;

    void Start()
    {
        // 1) Load from slot first
        int slot = PlayerCreationData.CurrentSlot;
        if (SaveSystem.TryLoadSlot(slot, out var save))
        {
            ApplyFromSave(save);
            Debug.Log($"Loaded Slot {slot}: {char_Race} {char_Class} {char_Name}");
            return;
        }

        // 2) Else use creation data (if you came from character creation)
        if (!string.IsNullOrWhiteSpace(PlayerCreationData.Name))
        {
            ApplyFromCreationData();
            Debug.Log($"Created (from creation): {char_Race} {char_Class} {char_Name}");
            return;
        }

        // 3) Else fallback
        ApplyFallbackDefaults();
        Debug.Log("No save + no creation data — using defaults");
    }

    private void ApplyFromCreationData()
    {
        char_Name = PlayerCreationData.Name;
        char_Race = PlayerCreationData.RaceId;
        char_Class = PlayerCreationData.ClassId;

        char_max_Health = PlayerCreationData.MaxHP;
        char_Damage = PlayerCreationData.Damage;
        char_INT = PlayerCreationData.INT;
        char_SPD = PlayerCreationData.SPD;
        char_Luck = PlayerCreationData.Luck;
        char_Charisma = PlayerCreationData.Charisma;

        char_Gold = PlayerCreationData.Gold;

        char_max_XP = (PlayerCreationData.MaxXP > 0) ? PlayerCreationData.MaxXP : 10;
        char_current_XP = Mathf.Clamp(PlayerCreationData.CurrentXP, 0, char_max_XP);

        char_current_Health = (PlayerCreationData.CurrentHP > 0)
            ? Mathf.Clamp(PlayerCreationData.CurrentHP, 0, char_max_Health)
            : char_max_Health;
    }


    private void ApplyFallbackDefaults()
    {
        char_Name = "Hero";
        char_Race = "Human";
        char_Class = "None";

        char_max_Health = 10;
        char_Damage = 1;
        char_INT = 5;
        char_SPD = 5;
        char_Luck = 10;
        char_Charisma = 10;
        char_max_XP = 10;

        char_current_Health = char_max_Health;
    }

    public void TakeDamage(int damage)
    {
        char_current_Health = Mathf.Max(0, char_current_Health - damage);
        Debug.Log(char_Name + " HP: " + char_current_Health);

        if (char_current_Health == 0)
        {
            Debug.Log(char_Name + " has been slain");
            if (uiController != null)
                uiController.UpdateGameUI(char_Name + " has been slain");
        }
    }

    public void AddGold(int amount)
    {
        char_Gold += amount;
    }
    public void AddXP(int amount)
    {
        char_current_XP += amount;
    }

    private bool TryLoadFromCurrentSlot(out PlayerSaveData save)
    {
        save = null;

        // IMPORTANT: you need this in PlayerCreationData
        // public static int CurrentSlot = 1;
        int slot = PlayerCreationData.CurrentSlot;

        return SaveSystem.TryLoadSlot(slot, out save);
    }

    private void ApplyFromSave(PlayerSaveData save)
    {
        char_Name = save.Name;
        char_Race = save.RaceId;
        char_Class = save.ClassId;

        char_max_Health = save.MaxHP;
        char_current_Health = Mathf.Clamp(save.CurrentHP, 0, save.MaxHP);

        char_Damage = save.Damage;
        char_INT = save.INT;
        char_SPD = save.SPD;

        char_Luck = save.Luck;
        char_Charisma = save.Charisma;

        char_Level = save.LVL;
        char_Gold = save.Gold;

        char_max_XP = (save.MaxXP > 0) ? save.MaxXP : 10;
        char_current_XP = Mathf.Clamp(save.CurrentXP, 0, char_max_XP);

        // keep PlayerCreationData in sync (optional but helpful)
        PlayerCreationData.Name = char_Name;
        PlayerCreationData.RaceId = char_Race;
        PlayerCreationData.ClassId = char_Class;

        PlayerCreationData.MaxHP = char_max_Health;
        PlayerCreationData.Damage = char_Damage;
        PlayerCreationData.INT = char_INT;
        PlayerCreationData.SPD = char_SPD;
        PlayerCreationData.LVL = char_Level;

        PlayerCreationData.Luck = char_Luck;
        PlayerCreationData.Charisma = char_Charisma;

        PlayerCreationData.Gold = char_Gold;
        PlayerCreationData.CurrentHP = char_current_Health;
        PlayerCreationData.CurrentXP = char_current_XP;
        PlayerCreationData.MaxXP = char_max_XP;

        PlayerCreationData.Traits.Clear();
        if (save.Traits != null) PlayerCreationData.Traits.AddRange(save.Traits);
    }

    public PlayerSaveData ToSaveData()
    {
        var save = new PlayerSaveData();

        save.Name = char_Name;
        save.RaceId = char_Race;
        save.ClassId = char_Class;

        save.MaxHP = char_max_Health;
        save.Damage = char_Damage;
        save.INT = char_INT;
        save.SPD = char_SPD;
        save.LVL = char_Level;

        save.Luck = char_Luck;
        save.Charisma = char_Charisma;

        save.Traits.Clear();
        // If you store traits somewhere else, copy them here.
        // If you're using PlayerCreationData.Traits as "truth":
        save.Traits.AddRange(PlayerCreationData.Traits);

        save.Gold = char_Gold;
        save.CurrentHP = char_current_Health;
        save.CurrentXP = char_current_XP;
        save.MaxXP = char_max_XP;

        return save;
    }

    public void ApplyItemEffects(ItemDef def)
    {
        if (def == null) return;

        foreach (var e in def.effects)
        {
            switch (e.type)
            {
                case ItemEffectType.AddMaxHP:
                    char_max_Health += e.amount;
                    char_current_Health = Mathf.Min(char_max_Health, char_current_Health + e.amount);
                    break;

                case ItemEffectType.AddDamage:
                    char_Damage += e.amount;
                    break;

                case ItemEffectType.AddINT:
                    char_INT += e.amount;
                    break;

                case ItemEffectType.AddSPD:
                    char_SPD += e.amount;
                    break;

                case ItemEffectType.AddLuck:
                    char_Luck += e.amount;
                    break;

                case ItemEffectType.AddCharisma:
                    char_Charisma += e.amount;
                    break;
            }
        }
    }


}
