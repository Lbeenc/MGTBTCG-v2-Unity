using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    public Character player;
    public UIController uiController;

    [Header("Scenes")]
    [SerializeField] private string combatSceneName = "SampleScene";
    [SerializeField] private string merchantSceneName = "MerchantScene";

    [Header("Wave Settings")]
    public int waveNumber = 1;
    [SerializeField] private int wavesPerMerchant = 3;

    [Header("Enemy Attack Settings")]
    private bool enemyAttackTurn;
    private bool playerAttackTurn;
    private bool enemyBlocked;
    private bool pendingNextWave;

    private bool inMerchant;
    private int nextWaveNumber;

    [Header("Buttons")]
    public Button attackButton;
    public Button blockButton;

    [Header("Enemy Spawning")]
    public Enemy[] enemyPrefabs;
    public Transform enemySpawnPoint;

    [SerializeField] private Enemy currentEnemy;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    void Start()
    {
        inMerchant = SceneManager.GetActiveScene().name == merchantSceneName;

        RebindSceneRefs();

        enemyAttackTurn = false;
        playerAttackTurn = true;

        if (player != null)
            player.char_current_Health = player.char_max_Health;

        if (inMerchant)
        {
            SetCombatEnabled(false);
        }
        else
        {
            SpawnEnemyForWave();
        }
    }

    void RebindSceneRefs()
    {
        player = FindFirstObjectByType<Character>();
        uiController = FindFirstObjectByType<UIController>();

        // Find buttons by name (or drag them in each scene)
        var atkGO = GameObject.Find("AttackButton");
        attackButton = atkGO ? atkGO.GetComponent<Button>() : null;

        var blkGO = GameObject.Find("BlockButton");
        blockButton = blkGO ? blkGO.GetComponent<Button>() : null;

        // Find spawn point by marker component (recommended)
        var sp = FindFirstObjectByType<EnemySpawnPointMarker>();
        enemySpawnPoint = sp ? sp.transform : null;

        // Re-wire listeners safely
        if (attackButton != null)
        {
            attackButton.onClick.RemoveAllListeners();
            attackButton.onClick.AddListener(PlayerAttack);
        }

        if (blockButton != null)
        {
            blockButton.onClick.RemoveAllListeners();
            blockButton.onClick.AddListener(PlayerBlock);
        }

        Debug.Log($"[Rebind] Scene={SceneManager.GetActiveScene().name} " +
                  $"player={(player != null)} ui={(uiController != null)} " +
                  $"spawn={(enemySpawnPoint != null)} atk={(attackButton != null)} blk={(blockButton != null)}");
    }




    void Update()
    {
        // Always keep buttons sane
        bool canClick = uiController != null && !uiController.waitingForInput;
        SetButtonsInteractable(!inMerchant && canClick && playerAttackTurn);

        // Merchant mode: NO combat loop, NO spawning, NO enemy attacks
        if (inMerchant) return;

        if (uiController != null && uiController.waitingForInput) return;
        if (player.char_current_Health <= 0) return;

        HandleEnemyAttack();

        // After messages finish, decide where to go next
        if (pendingNextWave && (uiController == null || !uiController.waitingForInput))
        {
            pendingNextWave = false;

            // Every 3 waves, go merchant BEFORE spawning next wave
            if ((nextWaveNumber - 1) % wavesPerMerchant == 0)
            {
                GoToMerchant();
                return;
            }

            waveNumber = nextWaveNumber;
            SpawnEnemyForWave();
        }
    }

    void SetButtonsInteractable(bool state)
    {
        if (attackButton != null) attackButton.interactable = state;
        if (blockButton != null) blockButton.interactable = state;
    }

    void SetCombatEnabled(bool enabled)
    {
        // Stops turns + stops enemy logic
        enemyAttackTurn = false;
        playerAttackTurn = enabled;
        enemyBlocked = false;

        // Optional: hide or disable buttons in merchant
        SetButtonsInteractable(enabled);
    }

    // -----------------------
    // Player Actions
    // -----------------------
    public void PlayerAttack()
    {
        if (inMerchant) return;
        if (uiController != null && uiController.waitingForInput) return;
        if (currentEnemy == null) return;

        if (currentEnemy.enemy_current_Health > 0 && playerAttackTurn)
        {
            currentEnemy.TakeDamage(player.char_Damage);
            uiController.UpdateGameUI("Player hit enemy for " + player.char_Damage);

            playerAttackTurn = false;
            enemyAttackTurn = true;

            if (currentEnemy.enemy_current_Health <= 0)
                OnEnemyDefeated();
        }
    }

    public void PlayerBlock()
    {
        if (inMerchant) return;
        if (uiController != null && uiController.waitingForInput) return;
        if (currentEnemy == null) return;

        if (currentEnemy.enemy_current_Health > 0 && playerAttackTurn)
        {
            enemyBlocked = true;
            playerAttackTurn = false;
            enemyAttackTurn = true;

            uiController.UpdateGameUI("Player blocked!");
        }
    }

    void HandleEnemyAttack()
    {
        if (inMerchant) return;
        if (currentEnemy == null || currentEnemy.enemy_current_Health <= 0) return;

        if (enemyAttackTurn)
        {
            currentEnemy.AttackPlayer(player);

            int dmgShown = enemyBlocked ? currentEnemy.enemy_Damage / 2 : currentEnemy.enemy_Damage;
            uiController.UpdateGameUI("Enemy hit player for " + dmgShown);

            enemyAttackTurn = false;
            playerAttackTurn = true;
            enemyBlocked = false;
        }
    }

    // -----------------------
    // Wave Flow
    // -----------------------
    void OnEnemyDefeated()
    {
        uiController.UpdateGameUI("Wave " + waveNumber + " cleared!");

        int goldReward = currentEnemy.goldReward;
        player.AddGold(goldReward);
        uiController.UpdateGameUI("Player gained " + goldReward + " gold.");

        int xpReward = currentEnemy.XPReward;
        player.AddXP(xpReward);
        uiController.UpdateGameUI("Player gained " + xpReward + " XP.");

        // lock turns until next wave starts
        SetCombatEnabled(false);

        // schedule next wave after dialog finishes
        nextWaveNumber = waveNumber + 1;
        pendingNextWave = true;
    }

    void SpawnEnemyForWave()
    {
        if (inMerchant) return;

        // If spawn point doesn't exist (wrong scene), do nothing instead of crashing/freezing.
        if (enemySpawnPoint == null)
        {
            Debug.LogWarning("EnemySpawnPoint missing (probably not in combat scene).");
            return;
        }

        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("No enemyPrefabs assigned in GameManager!");
            return;
        }

        if (currentEnemy != null)
            Destroy(currentEnemy.gameObject);

        int index = Mathf.Clamp(waveNumber - 1, 0, enemyPrefabs.Length - 1);
        var prefab = enemyPrefabs[index];

        currentEnemy = Instantiate(prefab, enemySpawnPoint.position, Quaternion.identity);

        enemyAttackTurn = false;
        playerAttackTurn = true;
        enemyBlocked = false;
    }

    // -----------------------
    // Merchant Flow
    // -----------------------
    void GoToMerchant()
    {
        inMerchant = true;
        SetCombatEnabled(false);

        if (currentEnemy != null)
        {
            Destroy(currentEnemy.gameObject);
            currentEnemy = null;
        }
        AutoSave("Going to Merchant");
        SceneManager.LoadScene(merchantSceneName);
    }

    public void ReturnFromMerchant()
    {
        // Call this from your merchant "Leave" button
        inMerchant = false;
        AutoSave("Leaving Merchant");
        SceneManager.LoadScene(combatSceneName);
        // Spawn will happen in OnSceneLoaded
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebindSceneRefs();

        inMerchant = (scene.name == merchantSceneName);

        if (inMerchant)
        {
            SetCombatEnabled(false);
            return;
        }

        // Combat scene
        SetCombatEnabled(true);

        if (pendingNextWave)
        {
            pendingNextWave = false;
            waveNumber = nextWaveNumber;
        }

        SpawnEnemyForWave();
    }


    public static PlayerSaveData BuildSaveFromCharacter(Character p)
    {
        var data = new PlayerSaveData();

        data.Name = p.char_Name;
        data.RaceId = p.char_Race;
        data.ClassId = p.char_Class;

        data.MaxHP = p.char_max_Health;
        data.Damage = p.char_Damage;
        data.INT = p.char_INT;
        data.SPD = p.char_SPD;
        data.LVL = p.char_Level;

        data.Luck = p.char_Luck;
        data.Charisma = p.char_Charisma;

        // if you're storing traits somewhere else, plug them in here
        data.Traits = new System.Collections.Generic.List<string>(PlayerCreationData.Traits);

        // NEW runtime stuff
        data.Gold = p.char_Gold;
        data.CurrentHP = p.char_current_Health;

        data.CurrentXP = p.char_current_XP;
        data.MaxXP = p.char_max_XP;

        return data;
    }

    public static void ApplySaveToCreationData(PlayerSaveData save)
    {
        PlayerCreationData.Name = save.Name;
        PlayerCreationData.RaceId = save.RaceId;
        PlayerCreationData.ClassId = save.ClassId;

        PlayerCreationData.MaxHP = save.MaxHP;
        PlayerCreationData.Damage = save.Damage;
        PlayerCreationData.INT = save.INT;
        PlayerCreationData.SPD = save.SPD;
        PlayerCreationData.LVL = save.LVL;

        PlayerCreationData.Luck = save.Luck;
        PlayerCreationData.Charisma = save.Charisma;

        PlayerCreationData.Traits.Clear();
        if (save.Traits != null) PlayerCreationData.Traits.AddRange(save.Traits);

        // NEW: store runtime values in PlayerCreationData too
        PlayerCreationData.Gold = save.Gold;
        PlayerCreationData.CurrentHP = save.CurrentHP;
        PlayerCreationData.CurrentXP = save.CurrentXP;
        PlayerCreationData.MaxXP = save.MaxXP;
    }
    private void AutoSave(string reason = "")
    {
        // If you don't have a slot picked yet, default to slot 1
        int slot = (PlayerCreationData.CurrentSlot <= 0) ? 1 : PlayerCreationData.CurrentSlot;

        if (player == null)
        {
            Debug.LogWarning($"Autosave skipped (player null). Reason: {reason}");
            return;
        }

        SaveSystem.SaveSlot(slot, player.ToSaveData());
        Debug.Log($"Autosaved slot {slot}. Reason: {reason}");
    }

    void EnterMerchantMode()
    {
        inMerchant = true;

        // No combat input
        if (attackButton != null) attackButton.interactable = false;
        if (blockButton != null) blockButton.interactable = false;

        // Destroy any existing enemy leftover
        if (currentEnemy != null)
        {
            Destroy(currentEnemy.gameObject);
            currentEnemy = null;
        }
    }

    void ExitMerchantMode()
    {
        inMerchant = false;
    }

}
