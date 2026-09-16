public static class PlayerCreationData
{
    public static int CurrentSlot = 1;

    public static string Name;
    public static string RaceId;
    public static string ClassId;

    // visible (base/derived)
    public static int MaxHP;
    public static int Damage;
    public static int INT;
    public static int SPD;
    public static int LVL;

    // hidden
    public static int Luck;
    public static int Charisma;

    public static readonly System.Collections.Generic.List<string> Traits
        = new System.Collections.Generic.List<string>();

    // NEW: runtime values
    public static int Gold;
    public static int CurrentHP;
    public static int CurrentXP;
    public static int MaxXP;
}
