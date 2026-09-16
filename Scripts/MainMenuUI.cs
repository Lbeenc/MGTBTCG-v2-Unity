using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "SampleScene";
    [SerializeField] private string CharacterCreationScene = "CharacterCreation";
    [SerializeField] private string CharacterSlotScene = "Character Slot Menu";

    public void OnStartButton()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnCharSlotButton()
    {
        SceneManager.LoadScene(CharacterSlotScene);
    }

    public void OnCharacterCreationButton()
    {
        SceneManager.LoadScene(CharacterCreationScene);
    }

    public void OnQuitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
