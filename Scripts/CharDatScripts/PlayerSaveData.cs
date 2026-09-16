using System;
using System.Collections.Generic;

[Serializable]
public class PlayerSaveData
{
    public string Name;
    public string RaceId;
    public string ClassId;

    // base/derived stats
    public int MaxHP;
    public int Damage;
    public int INT;
    public int SPD;
    public int LVL;

    public int Luck;
    public int Charisma;

    public List<string> Traits = new List<string>();

    // NEW: runtime values to persist
    public int Gold;
    public int CurrentHP;
    public int CurrentXP;
    public int MaxXP;
}

