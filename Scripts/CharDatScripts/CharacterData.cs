using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "RPG/Character Data")]
public class CharacterData : ScriptableObject
{
    public Sprite CharacterSprite;
    public string CharacterName;
    public string CharacterRace;
    public static int MaxHP;
    public static int Damage;
    public static int INT;
    public static int SPD;
    public static int LVL;
    public static int XP;

    // hidden
    public static int Luck;
    public static int Charisma;




    // Add anything else you want later (race, attack speed, etc.)
}