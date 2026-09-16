using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CharacterSlotMenu : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string characterCreationScene = "CharacterCreation";
    [SerializeField] private string gameScene = "SampleScene";

    [Header("Slot Buttons (size 5)")]
    [SerializeField] private Button[] slotButtons = new Button[5];

    [Header("Optional Slot Labels (size 5)")]
    [SerializeField] private TMP_Text[] slotLabels = new TMP_Text[5];

    [Header("Optional Delete Buttons (size 5)")]
    [SerializeField] private Button[] deleteButtons = new Button[5];

    [Header("Other")]
    [SerializeField] private Button quitButton;
    [SerializeField] private Button backButton;


    void Awake()
    {
        // Wire slot buttons
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int slotIndex = i + 1; // slots are 1..5
            if (slotButtons[i] != null)
                slotButtons[i].onClick.AddListener(() => ChooseSlot(slotIndex));

            if (deleteButtons != null && i < deleteButtons.Length && deleteButtons[i] != null)
                deleteButtons[i].onClick.AddListener(() => DeleteSlot(slotIndex));

        }

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
        if (backButton != null)
            backButton.onClick.AddListener(OnbackButton);
    }

    void Start()
    {
        RefreshUI();
    }

    public void ChooseSlot(int slot)
    {
        PlayerCreationData.CurrentSlot = slot;

        // If save exists, load straight into game
        if (SaveSystem.TryLoadSlot(slot, out var save))
        {
            // Put the save into PlayerCreationData so the game scene can use it too
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

            PlayerCreationData.Gold = save.Gold;
            PlayerCreationData.CurrentHP = save.CurrentHP;
            PlayerCreationData.CurrentXP = save.CurrentXP;
            PlayerCreationData.MaxXP = save.MaxXP;

            PlayerCreationData.Traits.Clear();
            if (save.Traits != null) PlayerCreationData.Traits.AddRange(save.Traits);

            SceneManager.LoadScene(gameScene);
        }
        else
        {
            // No save => go create a character for this slot
            SceneManager.LoadScene(characterCreationScene);
        }
    }

    public void DeleteSlot(int slot)
    {
        SaveSystem.DeleteSlot(slot);
        RefreshUI();
    }

    public void RefreshUI()
    {
        for (int i = 0; i < 5; i++)
        {
            int slot = i + 1;

            bool exists = SaveSystem.TryLoadSlot(slot, out PlayerSaveData save);

            // Update label text
            if (slotLabels != null && i < slotLabels.Length && slotLabels[i] != null)
            {
                if (!exists || save == null)
                {
                    slotLabels[i].text = $"Slot {slot} - EMPTY";
                }
                else
                {
                    // Customize display line however you want
                    string n = string.IsNullOrWhiteSpace(save.Name) ? "Hero" : save.Name;
                    slotLabels[i].text = $"Slot {slot} - {n} ({save.RaceId} {save.ClassId}) Lv {save.LVL}";
                }
            }

            // Optional: enable/disable delete button based on existence
            if (deleteButtons != null && i < deleteButtons.Length && deleteButtons[i] != null)
                deleteButtons[i].interactable = exists;
        }
    }

    private void ApplySaveToPlayerCreationData(PlayerSaveData save)
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
        if (save.Traits != null)
            PlayerCreationData.Traits.AddRange(save.Traits);
    }


    private void OnbackButton()
    {
        SceneManager.LoadScene(characterCreationScene);
    }
        
    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
