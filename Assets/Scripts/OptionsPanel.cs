using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Панель настроек: громкость музыки, громкость звуков, язык.
//
// Повесить на объект OptionsPanel. В инспекторе перетащить ползунки
// и (необязательно) кнопку языка с текстом. Значения сохраняются
// в PlayerPrefs и применяются сразу.
public class OptionsPanel : MonoBehaviour
{
    [Header("Ползунки (0..1)")]
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Подписи со значением в процентах (необязательно)")]
    public TMP_Text musicValueLabel;
    public TMP_Text sfxValueLabel;

    [Header("Язык")]
    public Button languageButton;      // кнопка-переключатель
    public TMP_Text languageLabel;     // текст на кнопке

    // Языки игры (см. Loc)
    public static string[] Languages => Loc.Names;
    public const string LangKey = Loc.Key;

    public static string CurrentLanguage => Loc.CurrentName;

    private void OnEnable()
    {
        SetupVolume();
        SetupLanguage();
    }

    private void SetupVolume()
    {
        var am = AudioManager.Instance;

        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
            musicSlider.SetValueWithoutNotify(
                am != null ? am.musicVolume : PlayerPrefs.GetFloat("vol_music", 0.6f));

            musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
            UpdateLabel(musicValueLabel, musicSlider.value);
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.SetValueWithoutNotify(
                am != null ? am.sfxVolume : PlayerPrefs.GetFloat("vol_sfx", 0.9f));

            sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
            sfxSlider.onValueChanged.AddListener(OnSfxChanged);
            UpdateLabel(sfxValueLabel, sfxSlider.value);
        }
    }

    private void SetupLanguage()
    {
        if (languageButton != null)
        {
            languageButton.onClick.RemoveListener(NextLanguage);
            languageButton.onClick.AddListener(NextLanguage);
        }
        RefreshLanguageLabel();
    }

    // ---- Обработчики ----

    public void OnMusicChanged(float v)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(v);
        else PlayerPrefs.SetFloat("vol_music", v);
        UpdateLabel(musicValueLabel, v);
    }

    public void OnSfxChanged(float v)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetSfxVolume(v);
        else PlayerPrefs.SetFloat("vol_sfx", v);
        UpdateLabel(sfxValueLabel, v);

        // Короткий звук для примера — слышно, что получилось
        AudioManager.PlaySfx("mouse");
        AudioManager.SuppressClick();
    }

    public void NextLanguage()
    {
        Loc.Next();              // переключает язык и уведомляет интерфейс
        RefreshLanguageLabel();
    }

    private void RefreshLanguageLabel()
    {
        if (languageLabel != null)
            languageLabel.text = Loc.UI("Language") + ": " + CurrentLanguage;
    }

    private static void UpdateLabel(TMP_Text label, float value01)
    {
        if (label != null) label.text = Mathf.RoundToInt(value01 * 100f) + "%";
    }
}
