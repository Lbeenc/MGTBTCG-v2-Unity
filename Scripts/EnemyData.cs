using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "RPG/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public Sprite enemySprite;
    public int maxHealth;
    public int minDamage;
    public int maxDamage;
    public int goldReward;
    public int XPReward;

    // Add anything else you want later (race, attack speed, etc.)
}