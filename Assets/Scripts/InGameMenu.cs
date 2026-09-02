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
        if (!OncePerFrame()) return;
        Loc.Next();
        RefreshLanguageLabel();
    }

    private void RefreshLanguageLabel()
    {
        if (languageLabel != null)
            languageLabel.text = Loc.UI("Language") + ": " + Loc.CurrentName;
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

    // Открыта ли панель настроек
    public bool OptionsOpen => optionsBox != null && optionsBox.activeSelf;

    // Защита от двойного срабатывания: ручная проверка ловит нажатие,
    // а обычный OnClick кнопки — отпускание, и это разные кадры.
    // Поэтому игнорируем повторные срабатывания в течение 0.3 секунды.
    private float lastActionTime = -1f;
    private bool OncePerFrame()
    {
        float now = Time.unscaledTime;
        if (now - lastActionTime < 0.3f) return false;
        lastActionTime = now;
        return true;
    }

    public void Toggle()
    {
        if (menuPanel == null) return;
        if (!OncePerFrame()) return;
        if (menuPanel.activeSelf) Close(); else Open();
    }

    public void Open()
    {
        if (menuPanel != null) menuPanel.SetActive(true);
        if (pauseGame) Time.timeScale = 0f;
    }

    public void Close()
    {
        if (!OncePerFrame()) return;
        if (menuPanel != null) menuPanel.SetActive(false);
        if (optionsBox != null) optionsBox.SetActive(false);
        if (pauseGame) Time.timeScale = 1f;
    }

    public void ToggleOptions()
    {
        if (!OncePerFrame()) return;
        if (optionsBox != null) optionsBox.SetActive(!optionsBox.activeSelf);
    }

    public void GoToMainMenu()
    {
        if (OptionsOpen) return;   // пока открыты настройки — выход недоступен
        if (!OncePerFrame()) return;
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
        {
            Toggle();
            return;
        }

        // Запасной путь: ловим клик по прямоугольнику кнопки сами.
        // Работает, даже если сверху лежит другой канвас или картинка.
        if (!Input.GetMouseButtonDown(0)) return;

        bool panelOpen = menuPanel != null && menuPanel.activeSelf;

        if (!panelOpen)
        {
            if (PointerOver(menuIcon)) Toggle();
            return;
        }

        if (OptionsOpen)                       // за настройками кнопки недоступны
        {
            if (PointerOver(languageButton)) NextLanguage();
            return;
        }

        if (PointerOver(resumeButton)) { Close(); return; }
        if (PointerOver(optionsButton)) { ToggleOptions(); return; }
        if (PointerOver(mainMenuButton)) { GoToMainMenu(); return; }
    }

    private bool PointerOver(Component c)
    {
        if (c == null || !c.gameObject.activeInHierarchy) return false;

        var rt = c.GetComponent<RectTransform>();
        if (rt == null) return false;

        var canvas = rt.GetComponentInParent<Canvas>();
        Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? canvas.worldCamera : null;

        return RectTransformUtility.RectangleContainsScreenPoint(rt, Input.mousePosition, cam);
    }

    private void OnDestroy()
    {
        // на случай выхода из сцены с открытым меню
        if (pauseGame) Time.timeScale = 1f;
    }
}
