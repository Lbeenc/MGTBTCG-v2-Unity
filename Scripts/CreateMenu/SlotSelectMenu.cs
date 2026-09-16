using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SlotSelectMenu : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string characterCreationScene = "CharacterCreation";
    [SerializeField] private string gameScene = "SampleScene";

    [Header("UI")]
    public TMP_Text selectedSlotText;
    public TMP_Text slotSummaryText;

    public GameObject newButton;
    public GameObject loadButton;
    public GameObject deleteButton;

    private int selectedSlot = 1;

    void Start()
    {
        SelectSlot(1);
    }

    // Called by clicking Slot 1..5 buttons
    public void SelectSlot(int slot)
    {
        selectedSlot = slot;
        PlayerCreationData.CurrentSlot = slot;

        if (selectedSlotText != null)
            selectedSlotText.text = $"Slot {slot}";

        RefreshPanel();
    }

    void RefreshPanel()
    {
        bool exists = SaveSystem.TryLoadSlot(selectedSlot, out var save);

        if (!exists)
        {
            if (slotSummaryText != null)
                slotSummaryText.text = "Empty Slot";

            SetActive(newButton, true);
            SetActive(loadButton, false);
            SetActive(deleteButton, false);
            return;
        }

        if (slotSummaryText != null)
        {
            slotSummaryText.text =
                $"{save.Name}\n{save.RaceId} {save.ClassId}\nLVL {save.LVL}";
        }

        SetActive(newButton, false);
        SetActive(loadButton, true);
        SetActive(deleteButton, true);
    }

    public void NewCharacter()
    {
        // wipe current in-memory data, keep selected slot
        PlayerCreationData.Name = "";
        PlayerCreationData.RaceId = "";
        PlayerCreationData.ClassId = "";
        PlayerCreationData.MaxHP = 0;
        PlayerCreationData.Damage = 0;
        PlayerCreationData.INT = 0;
        PlayerCreationData.SPD = 0;
        PlayerCreationData.LVL = 1;
        PlayerCreationData.Luck = 0;
        PlayerCreationData.Charisma = 0;
        PlayerCreationData.Traits.Clear();

        SceneManager.LoadScene(characterCreationScene);
    }

    public void LoadCharacter()
    {

        if (!SaveSystem.TryLoadSlot(selectedSlot, out var save))
        {
            RefreshPanel();
            return;
        }

        GameManager.ApplySaveToCreationData(save);
        SceneManager.LoadScene(gameScene);
    }

    public void DeleteCharacter()
    {
        SaveSystem.DeleteSlot(selectedSlot);
        RefreshPanel();
    }

    private void ApplySaveToCreationData(PlayerSaveData save)
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
    }

    private void SetActive(GameObject obj, bool active)
    {
        if (obj != null) obj.SetActive(active);
    }
}
