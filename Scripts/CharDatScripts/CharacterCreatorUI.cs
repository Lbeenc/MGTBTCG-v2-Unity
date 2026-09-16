using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class CharacterCreationUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField nameInput;
    public TMP_Dropdown classDropdown;
    public TMP_Dropdown raceDropdown;
    public Button Next_Button;
    public Button backButton;

    [Header("Next Scene")]
    [SerializeField] private string GameplayScene = "SampleScene";
    
    [Header("Previous Scene")]
    [SerializeField] private string MainMenuScene = "MainMenu";
   
    // Base stats (visible)
    private const int BASE_HP = 5;
    private const int BASE_DMG = 1;
    private const int BASE_INT = 5;
    private const int BASE_SPD = 5;

    // Base stats (hidden)
    private const int BASE_LUCK = 10;
    private const int BASE_CHA = 10;

    private static readonly List<string> ClassOptions = new()
    {
        "Knight", "Archer", "Wizard", "None"
    };

    private static readonly List<string> RaceOptions = new()
    {
        "Human", "Elf", "Orc", "Cow"
    };

   
    private void FillDropdown(TMP_Dropdown dd, List<string> options, string defaultValue)
    {
        if (dd == null) return;

        dd.ClearOptions();
        dd.AddOptions(options);

        int idx = options.IndexOf(defaultValue);
        dd.value = (idx >= 0) ? idx : 0;

        dd.RefreshShownValue();
    }
    void Start()
    {
        backButton.onClick.AddListener(OnBackButton);

        FillDropdown(classDropdown, ClassOptions, "None");
        FillDropdown(raceDropdown, RaceOptions, "Human");

        if (classDropdown != null)
        {
            classDropdown.ClearOptions();
            classDropdown.AddOptions(new List<string> { "Knight", "Archer", "Wizard", "None" });
        }

        if (raceDropdown != null)
        {
            raceDropdown.ClearOptions();
            raceDropdown.AddOptions(new List<string> { "Human", "Elf", "Orc", "Cow" });
        }
    }
    struct StatMods
    {
        public float hp, dmg, intel, spd, luck, cha;
        public List<string> traits;

        public StatMods(float hp, float dmg, float intel, float spd, float luck, float cha, List<string> traits = null)
        {
            this.hp = hp; this.dmg = dmg; this.intel = intel; this.spd = spd; this.luck = luck; this.cha = cha;
            this.traits = traits ?? new List<string>();
        }
    }

    public void OnConfirmButton()
    {
        string chosenName = nameInput != null ? nameInput.text : "";
        if (string.IsNullOrWhiteSpace(chosenName)) chosenName = "Hero";

        string classId = GetDropdownText(classDropdown, "None");
        string raceId = GetDropdownText(raceDropdown, "Human");

        PlayerCreationData.Name = chosenName;
        PlayerCreationData.ClassId = classId;
        PlayerCreationData.RaceId = raceId;

        // 1) get multipliers
        StatMods raceMods = GetRaceMods(raceId);
        StatMods classMods = GetClassMods(classId);

        // 2) combine multipliers
        float hpM = raceMods.hp * classMods.hp;
        float dmgM = raceMods.dmg * classMods.dmg;
        float intM = raceMods.intel * classMods.intel;
        float spdM = raceMods.spd * classMods.spd;
        float luckM = raceMods.luck * classMods.luck;
        float chaM = raceMods.cha * classMods.cha;

        // 3) apply to base stats
        PlayerCreationData.MaxHP = Mathf.RoundToInt(BASE_HP * hpM);
        PlayerCreationData.Damage = Mathf.RoundToInt(BASE_DMG * dmgM);
        PlayerCreationData.INT = Mathf.RoundToInt(BASE_INT * intM);
        PlayerCreationData.SPD = Mathf.RoundToInt(BASE_SPD * spdM);

        PlayerCreationData.Luck = Mathf.RoundToInt(BASE_LUCK * luckM);
        PlayerCreationData.Charisma = Mathf.RoundToInt(BASE_CHA * chaM);

        // 4) traits (race traits + any class traits)
        PlayerCreationData.Traits.Clear();
        PlayerCreationData.Traits.AddRange(raceMods.traits);
        PlayerCreationData.Traits.AddRange(classMods.traits);

        // optional: clamp INT/SPD so they never go negative
        PlayerCreationData.INT = Mathf.Max(0, PlayerCreationData.INT);
        PlayerCreationData.SPD = Mathf.Max(0, PlayerCreationData.SPD);

        var save = new PlayerSaveData
        {
            Name = PlayerCreationData.Name,
            RaceId = PlayerCreationData.RaceId,
            ClassId = PlayerCreationData.ClassId,

            MaxHP = PlayerCreationData.MaxHP,
            Damage = PlayerCreationData.Damage,
            INT = PlayerCreationData.INT,
            SPD = PlayerCreationData.SPD,
            LVL = PlayerCreationData.LVL,

            Luck = PlayerCreationData.Luck,
            Charisma = PlayerCreationData.Charisma,

            Traits = new System.Collections.Generic.List<string>(PlayerCreationData.Traits)
        };

        SaveSystem.SavePlayer(save);
        // then load game scene

        SceneManager.LoadScene(GameplayScene);
    }


    public void OnBackButton()
    {
        SceneManager.LoadScene(MainMenuScene);
    }
    private static string GetDropdownText(TMP_Dropdown dd, string fallback)
    {
        if (dd == null || dd.options == null || dd.options.Count == 0) return fallback;
        return dd.options[dd.value].text;
    }

    private static StatMods GetRaceMods(string raceId)
    {
        switch (raceId)
        {
            case "Human":
                return new StatMods(
                    hp: 1.5f, dmg: 2f, intel: 2f, spd: 2f,
                    luck: 4f, cha: 3f
                );

            case "Elf":
                return new StatMods(
                    hp: 1.25f, dmg: 1f, intel: 3f, spd: 2f,
                    luck: 5f, cha: 1.5f,
                    traits: new List<string> { "Nature" }
                );

            case "Orc":
                return new StatMods(
                    hp: 2.5f, dmg: 2f, intel: 1f, spd: 0.5f,
                    luck: 2f, cha: 0.5f,
                    traits: new List<string> { "Bloodlust" }
                );

            case "Cow":
                return new StatMods(
                    hp: 3f, dmg: 2f, intel: 0f, spd: 2f,
                    luck: 5f, cha: 5f,
                    traits: new List<string> { "Nature", "Polish", "Cocaine?" }
                );

            default:
                return new StatMods(1f, 1f, 1f, 1f, 1f, 1f);
        }
    }

    private static StatMods GetClassMods(string classId)
    {
        switch (classId)
        {
            case "Knight":
                return new StatMods(
                    hp: 1.5f, dmg: 2f, intel: 1f, spd: 0.75f,
                    luck: 1f, cha: 1f
                );

            case "Archer":
                return new StatMods(
                    hp: 1f, dmg: 2f, intel: 1f, spd: 2f,
                    luck: 1f, cha: 1f
                );

            case "Wizard":
                return new StatMods(
                    hp: 0.75f, dmg: 0.5f, intel: 10f, spd: 1f,
                    luck: 1f, cha: 1f
                );

            case "None":
                // you said None gives luck/charisma x2 also
                return new StatMods(
                    hp: 1.25f, dmg: 1.25f, intel: 1.25f, spd: 1.25f,
                    luck: 2f, cha: 2f
                );

            default:
                return new StatMods(1f, 1f, 1f, 1f, 1f, 1f);
        }
    }
}
