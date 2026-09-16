using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public EnemyData data;

    [Header("Runtime Stats")]
    public int enemy_current_Health;
    public int enemy_Damage;
    public int goldReward;
    public int XPReward;

    [Header("HP UI")]
    public GameObject hpUIPrefab;
    private Transform hpUI;
    private Image hpBarImage;
    private TMP_Text hpText; 


    void Start()
    {
        enemy_current_Health = data.maxHealth;
        enemy_Damage = data.maxDamage;

        goldReward = data.goldReward;  
        XPReward = data.XPReward;
        if (hpUIPrefab == null)
        {
            Debug.LogError($"{name}: hpUIPrefab is NOT assigned on Enemy!");
            return;
        }

        GameObject canvas = GameObject.Find("WorldCanvas");
        if (canvas == null)
        {
            Debug.LogError("WorldCanvas not found in scene!");
            return;
        }

        GameObject ui = Instantiate(hpUIPrefab, canvas.transform);

        hpUI = ui.GetComponent<Transform>();

        
        hpBarImage = ui.transform.Find("EnemyHPBar").GetComponent<Image>();
        hpText = ui.transform.Find("EnemyHPText").GetComponent<TMP_Text>();

        if (hpBarImage == null || hpText == null)
        {
            Debug.LogError($"{name}: HP UI is missing Image or TMP_Text component");
        }
    }

    public void TakeDamage(int amount)
    {
        enemy_current_Health = Mathf.Max(0, enemy_current_Health - amount);
        Debug.Log($"{name} HP: {enemy_current_Health}/{data.maxHealth}");
        UpdateHPUI();
    }

    public void AttackPlayer(Character player)
    {
        int dmg = data.maxDamage;
        enemy_Damage = dmg;

        player.TakeDamage(dmg);
        Debug.Log($"{name} attacked player for {dmg} damage!");
    }

    void Update()
    {
       UpdateHPUI();
    }

    public void UpdateHPUI()
    {

        if (hpUI == null || hpBarImage == null || hpText == null || data == null || Camera.main == null)
            return;
        Vector3 worldPos = transform.position + new Vector3(0, 1.5f, 0);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        hpUI.position = screenPos;

        float percent = (float)enemy_current_Health / data.maxHealth;
        hpBarImage.fillAmount = percent;

        hpText.text = enemy_current_Health + "/" + data.maxHealth;

        Debug.Log($"{name} UI: HP={enemy_current_Health}/{data.maxHealth}, fill={percent}");
    }


    void OnDestroy()
    {
        if (hpUI != null)
            Destroy(hpUI.gameObject);
    }
}
