using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Финальная анимация в хабе: после последнего разговора с птицей
// картинки появляются одна за другой, каждая через вспышку.
//
// Повесить на объект final_animation (тот, что в Canvas).
// Дочерние Image берутся автоматически в порядке иерархии.
public class FinalAnimation : MonoBehaviour
{
    [Header("Картинки (пусто = все дочерние Image по порядку)")]
    public Image[] pictures;

    [Header("Вспышка")]
    [Tooltip("Белый прямоугольник на весь экран. Пусто — создастся сам")]
    public Image flash;
    public Color flashColor = Color.white;
    public float flashIn = 0.12f;     // как быстро вспыхивает
    public float flashOut = 0.45f;    // как гаснет

    [Header("Ритм")]
    public float delayBeforeStart = 0.6f;  // пауза после диалога
    public float delayBetween = 0.9f;      // пауза между картинками
    public float pictureFadeIn = 0.3f;     // проявление самой картинки

    [Header("Звук появления картинки (из Resources/audio, можно пусто)")]
    public string flashSfx = "tree_transform";

    [Header("Музыка финала (из Resources/audio, можно пусто)")]
    public string musicTrack = "final_scene";

    [Header("Кнопки внизу (появляются после всех картинок)")]
    public GameObject buttonsRoot;      // объект с кнопками Back и Main Menu
    public Button backButton;           // закрыть картинки, остаться в саду
    public Button mainMenuButton;       // выйти в главное меню
    public float buttonsFadeIn = 0.5f;
    public string mainMenuScene = "00_MainMenu";

    public bool IsPlaying { get; private set; }
    private bool played;

    void Awake()
    {
        AutoWireButtons();
        CollectPictures();
        HideAll();
        EnsureFlash();

        if (buttonsRoot != null) buttonsRoot.SetActive(false);
    }

    // Находит кнопки по именам, если они не назначены вручную
    private void AutoWireButtons()
    {
        if (buttonsRoot == null) buttonsRoot = FindChild("FinalButtons", "Buttons")?.gameObject;
        if (backButton == null) backButton = FindChild("BackButton", "Back")?.GetComponent<Button>();
        if (mainMenuButton == null) mainMenuButton = FindChild("MainMenuButton", "MainMenu")?.GetComponent<Button>();

        if (backButton != null) { backButton.onClick.RemoveListener(OnBack); backButton.onClick.AddListener(OnBack); }
        if (mainMenuButton != null) { mainMenuButton.onClick.RemoveListener(OnMainMenu); mainMenuButton.onClick.AddListener(OnMainMenu); }
    }

    private Transform FindChild(params string[] names)
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
            foreach (var n in names)
                if (string.Equals(t.name.Trim(), n, System.StringComparison.OrdinalIgnoreCase))
                    return t;
        return null;
    }

    // Убирает картинки, но игра остаётся пройденной — сад никуда не девается
    public void OnBack()
    {
        gameObject.SetActive(false);
    }

    public void OnMainMenu()
    {
        SceneTransition.Load(mainMenuScene);
    }

    private void CollectPictures()
    {
        if (pictures != null && pictures.Length > 0) return;

        var list = new List<Image>();
        foreach (Transform child in transform)
        {
            var img = child.GetComponent<Image>();
            if (img != null && img != flash) list.Add(img);
        }
        pictures = list.ToArray();
    }

    private void HideAll()
    {
        foreach (var p in pictures)
            if (p != null) p.gameObject.SetActive(false);
    }

    // Белая вспышка на весь экран поверх картинок
    private void EnsureFlash()
    {
        if (flash != null) { SetAlpha(flash, 0f); return; }

        var go = new GameObject("Flash", typeof(RectTransform));
        go.transform.SetParent(transform, false);
        go.transform.SetAsLastSibling(); // поверх всех картинок

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        flash = go.AddComponent<Image>();
        flash.color = flashColor;
        flash.raycastTarget = false;
        SetAlpha(flash, 0f);
    }

    // Главный вход: запустить финальную анимацию
    public void Play()
    {
        if (played || IsPlaying) return;
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        IsPlaying = true;
        played = true;

        // Музыка финала (плавно сменит текущую)
        if (!string.IsNullOrEmpty(musicTrack) && AudioManager.Instance != null)
            AudioManager.Instance.PlayMusic(musicTrack);

        yield return new WaitForSeconds(delayBeforeStart);

        foreach (var pic in pictures)
        {
            if (pic == null) continue;

            // вспышка нарастает
            if (!string.IsNullOrEmpty(flashSfx)) AudioManager.PlaySfx(flashSfx);
            yield return FadeImage(flash, 0f, 1f, flashIn);

            // на пике вспышки включаем картинку
            pic.gameObject.SetActive(true);
            SetAlpha(pic, 0f);

            // вспышка гаснет, картинка проявляется
            StartCoroutine(FadeImage(pic, 0f, 1f, pictureFadeIn));
            yield return FadeImage(flash, 1f, 0f, flashOut);

            yield return new WaitForSeconds(delayBetween);
        }

        // Все картинки на месте — показываем кнопки, картинки остаются на экране
        yield return ShowButtons();

        IsPlaying = false;
    }

    private IEnumerator ShowButtons()
    {
        if (buttonsRoot == null) yield break;

        buttonsRoot.SetActive(true);

        var group = buttonsRoot.GetComponent<CanvasGroup>();
        if (group == null) group = buttonsRoot.AddComponent<CanvasGroup>();

        group.alpha = 0f;
        for (float t = 0f; t < buttonsFadeIn; t += Time.deltaTime)
        {
            group.alpha = t / buttonsFadeIn;
            yield return null;
        }
        group.alpha = 1f;
    }

    private IEnumerator FadeImage(Image img, float from, float to, float time)
    {
        if (img == null || time <= 0f) { if (img != null) SetAlpha(img, to); yield break; }

        for (float t = 0f; t < time; t += Time.deltaTime)
        {
            SetAlpha(img, Mathf.Lerp(from, to, t / time));
            yield return null;
        }
        SetAlpha(img, to);
    }

    private static void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        var c = img.color;
        c.a = a;
        img.color = c;
    }
}
