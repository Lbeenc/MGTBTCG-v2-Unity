using System;
using System.Collections.Generic;
using UnityEngine;

public enum ItemEffectType
{
    AddMaxHP,
    AddDamage,
    AddINT,
    AddSPD,
    AddLuck,
    AddCharisma
}

[CreateAssetMenu(menuName = "Game/ItemData (All Items)")]
public class ItemData : ScriptableObject
{
    public List<ItemDef> items = new List<ItemDef>();

    public ItemDef Get(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return items.Find(x => x != null && string.Equals(x.id, id, StringComparison.OrdinalIgnoreCase));
    }

    public int GetOwnedStacks(string id)
    {
        // using your existing save pipeline: PlayerCreationData.Traits holds owned item IDs
        int count = 0;
        if (PlayerCreationData.Traits == null) return 0;
        for (int i = 0; i < PlayerCreationData.Traits.Count; i++)
            if (string.Equals(PlayerCreationData.Traits[i], id, StringComparison.OrdinalIgnoreCase))
                count++;
        return count;
    }
}

[Serializable]
public class ItemDef
{
    [Header("Identity")]
    public string id;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Shop")]
    public int cost = 10;

    [Header("Stacking")]
    public bool canStack = true;
    public int maxStacks = 999;

    [Header("Effects (applied per purchase/stack)")]
    public List<ItemEffectDef> effects = new List<ItemEffectDef>();
}

[Serializable]
public class ItemEffectDef
{
    public ItemEffectType type;
    public int amount;
}
