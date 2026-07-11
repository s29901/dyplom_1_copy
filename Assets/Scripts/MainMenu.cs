using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Главное меню. Повесить на пустой объект в сцене 00_MainMenu.
// Кнопки подключаются через OnClick: Play -> OnPlay, New Game -> OnNewGame,
// Options -> OnOptions, Credits -> OnCredits, Exit -> OnExit.
// Кнопка "Start" на панели имени -> OnConfirmName.
public class MainMenu : MonoBehaviour
{
    [Header("Сцены")]
    public string firstSceneName = "02_HubGarden"; // куда ведёт новая игра (можно 01_Intro)
    public string hubSceneName = "02_HubGarden";   // куда ведёт Play (продолжить)

    [Header("Панели (выключенные)")]
    public GameObject namePanel;    // панель ввода имени
    public GameObject optionsPanel; // панель настроек
    public GameObject creditsPanel; // панель титров

    [Header("Элементы")]
    public TMP_InputField nameInput; // поле ввода имени на namePanel
    public Button playButton;        // кнопка Play
    public Button startButton;       // кнопка Start на панели имени — гаснет при пустом поле
    public Slider volumeSlider;      // ползунок громкости на optionsPanel (необязательно)

    private void Start()
    {
        CloseAllPanels();

        // Start недоступна, пока имя не введено
        if (startButton != null && nameInput != null)
        {
            startButton.interactable = false;
            nameInput.onValueChanged.AddListener(
                v => startButton.interactable = !string.IsNullOrWhiteSpace(v));
        }

        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("volume", 1f);
            AudioListener.volume = volumeSlider.value;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    // ---- Кнопки ----

    public void OnPlay()
    {
        // Если игра ещё ни разу не начиналась — Play работает как New Game
        if (PlayerPrefs.GetInt("game_started", 0) == 0)
        {
            OnNewGame();
            return;
        }
        SceneManager.LoadScene(hubSceneName);
    }

    public void OnNewGame()
    {
        CloseAllPanels();

        if (namePanel != null)
        {
            namePanel.SetActive(true);
            if (nameInput != null) nameInput.text = "";
        }
        else
        {
            // Панель имени не назначена — начинаем сразу с именем по умолчанию
            Debug.LogWarning("MainMenu: Name Panel не назначена, начинаю новую игру без ввода имени");
            OnConfirmName();
        }
    }

    // Кнопка "Start" на панели имени
    public void OnConfirmName()
    {
        // Сбрасываем весь прогресс (громкость сохраняем)
        float volume = PlayerPrefs.GetFloat("volume", 1f);
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetFloat("volume", volume);

        string playerName = nameInput != null ? nameInput.text.Trim() : "Hero";
        if (string.IsNullOrEmpty(playerName)) return; // без имени не начинаем
        PlayerPrefs.SetString("player_name", playerName);
        PlayerPrefs.SetInt("game_started", 1);
        PlayerPrefs.Save();

        // Если ProgressManager уже жив с прошлой игры — обнуляем его
        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.quest1Done = false;
            ProgressManager.Instance.quest2Done = false;
            ProgressManager.Instance.quest3Done = false;
            ProgressManager.Instance.quest4Done = false;
        }

        SceneManager.LoadScene(firstSceneName);
    }

    public void OnOptions()
    {
        bool show = optionsPanel != null && !optionsPanel.activeSelf;
        CloseAllPanels();
        if (optionsPanel != null) optionsPanel.SetActive(show);
    }

    public void OnCredits()
    {
        bool show = creditsPanel != null && !creditsPanel.activeSelf;
        CloseAllPanels();
        if (creditsPanel != null) creditsPanel.SetActive(show);
    }

    public void OnExit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void CloseAllPanels()
    {
        if (namePanel != null) namePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    private void SetVolume(float v)
    {
        AudioListener.volume = v;
        PlayerPrefs.SetFloat("volume", v);
        PlayerPrefs.Save();
    }
}
