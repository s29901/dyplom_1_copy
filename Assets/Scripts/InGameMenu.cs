using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Игровое меню внутри сцены (иконка в углу + панель с кнопками).
//
// Интерфейс собираешь сама на своём Canvas, а этот скрипт связывает всё вместе.
// Повесить на любой объект сцены (удобно — на сам Canvas) и заполнить поля.
//
// Что должно быть на сцене:
//   MenuIcon      — кнопка-иконка в левом верхнем углу
//   MenuPanel     — панель меню (выключена по умолчанию)
//     ResumeButton    — закрыть меню
//     OptionsButton   — показать/скрыть настройки
//     MainMenuButton  — выйти в главное меню
//   OptionsBox    — блок с ползунками (выключен по умолчанию)
//     MusicSlider, SfxSlider
//
// Поля можно не заполнять: если объекты названы как выше,
// скрипт найдёт их сам среди дочерних объектов.
public class InGameMenu : MonoBehaviour
{
    [Header("Сцена главного меню")]
    public string mainMenuScene = "00_MainMenu";

    [Header("Основные элементы")]
    public Button menuIcon;          // иконка в углу
    public GameObject menuPanel;     // панель меню

    [Header("Кнопки внутри панели")]
    public Button resumeButton;
    public Button optionsButton;
    public Button mainMenuButton;

    [Header("Настройки звука")]
    public GameObject optionsBox;    // блок с ползунками
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Язык")]
    public Button languageButton;
    public TMPro.TMP_Text languageLabel;

    [Header("Поведение")]
    public bool pauseGame = true;        // ставить игру на паузу, пока меню открыто
    public bool escapeToggles = true;    // открывать/закрывать по Esc

    private void Start()
    {
        AutoWire();
        BindButtons();
        BindSliders();

        if (menuPanel != null) menuPanel.SetActive(false);
        if (optionsBox != null) optionsBox.SetActive(false);
        if (pauseGame) Time.timeScale = 1f;
    }

    // Ищет объекты по именам, если они не назначены вручную.
    // Регистр букв не важен, у каждого поля несколько допустимых имён.
    private void AutoWire()
    {
        if (menuIcon == null) menuIcon = FindChild<Button>("MenuIcon", "MenuButton");
        if (menuPanel == null) menuPanel = FindChild<Transform>("MenuPanel", "Panel")?.gameObject;
        if (resumeButton == null) resumeButton = FindChild<Button>("ResumeButton", "Resume");
        if (optionsButton == null) optionsButton = FindChild<Button>("OptionsButton");
        if (mainMenuButton == null) mainMenuButton = FindChild<Button>("MainMenuButton");
        if (optionsBox == null) optionsBox = FindChild<Transform>("OptionsBox", "OptionsPanel")?.gameObject;
        if (musicSlider == null) musicSlider = FindChild<Slider>("MusicSlider", "MusicVolume");
        if (sfxSlider == null) sfxSlider = FindChild<Slider>("SfxSlider", "SFXSlider", "SoundSlider");
        if (languageButton == null) languageButton = FindChild<Button>("LanguageButton");
        if (languageLabel == null && languageButton != null)
            languageLabel = languageButton.GetComponentInChildren<TMPro.TMP_Text>(true);
    }

    private T FindChild<T>(params string[] names) where T : Component
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            string n = t.name.Trim();
            foreach (var name in names)
                if (string.Equals(n, name, System.StringComparison.OrdinalIgnoreCase))
                {
                    var c = t.GetComponent<T>();
                    if (c != null) return c;
                }
        }
        return null;
    }

    private void BindButtons()
    {
        if (menuIcon != null) { menuIcon.onClick.RemoveListener(Toggle); menuIcon.onClick.AddListener(Toggle); }
        if (resumeButton != null) { resumeButton.onClick.RemoveListener(Close); resumeButton.onClick.AddListener(Close); }
        if (optionsButton != null) { optionsButton.onClick.RemoveListener(ToggleOptions); optionsButton.onClick.AddListener(ToggleOptions); }
        if (mainMenuButton != null) { mainMenuButton.onClick.RemoveListener(GoToMainMenu); mainMenuButton.onClick.AddListener(GoToMainMenu); }
        if (languageButton != null) { languageButton.onClick.RemoveListener(NextLanguage); languageButton.onClick.AddListener(NextLanguage); }
        RefreshLanguageLabel();
    }

    public void NextLanguage()
    {
        int i = PlayerPrefs.GetInt(OptionsPanel.LangKey, 0);
        i = (i + 1) % OptionsPanel.Languages.Length;
        PlayerPrefs.SetInt(OptionsPanel.LangKey, i);
        PlayerPrefs.Save();
        RefreshLanguageLabel();
    }

    private void RefreshLanguageLabel()
    {
        if (languageLabel != null)
            languageLabel.text = "Language: " + OptionsPanel.CurrentLanguage;
    }

    private void BindSliders()
    {
        var am = AudioManager.Instance;

        if (musicSlider != null)
        {
            musicSlider.minValue = 0f; musicSlider.maxValue = 1f;
            musicSlider.SetValueWithoutNotify(am != null ? am.musicVolume : PlayerPrefs.GetFloat("vol_music", 0.6f));
            musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f; sfxSlider.maxValue = 1f;
            sfxSlider.SetValueWithoutNotify(am != null ? am.sfxVolume : PlayerPrefs.GetFloat("vol_sfx", 0.9f));
            sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
            sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        }
    }

    // ---------- Публичные методы (можно вешать на кнопки вручную) ----------

    public void Toggle()
    {
        if (menuPanel == null) return;
        if (menuPanel.activeSelf) Close(); else Open();
    }

    public void Open()
    {
        if (menuPanel != null) menuPanel.SetActive(true);
        if (pauseGame) Time.timeScale = 0f;
    }

    public void Close()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        if (optionsBox != null) optionsBox.SetActive(false);
        if (pauseGame) Time.timeScale = 1f;
    }

    public void ToggleOptions()
    {
        if (optionsBox != null) optionsBox.SetActive(!optionsBox.activeSelf);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        if (menuPanel != null) menuPanel.SetActive(false);
        SceneTransition.Load(mainMenuScene);
    }

    public void OnMusicChanged(float v)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(v);
        else PlayerPrefs.SetFloat("vol_music", v);
    }

    public void OnSfxChanged(float v)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetSfxVolume(v);
        else PlayerPrefs.SetFloat("vol_sfx", v);
    }

    private void Update()
    {
        if (escapeToggles && Input.GetKeyDown(KeyCode.Escape))
            Toggle();
    }

    private void OnDestroy()
    {
        // на случай выхода из сцены с открытым меню
        if (pauseGame) Time.timeScale = 1f;
    }
}
