using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Короткая анимация перехода между сценами.
//
// Ничего настраивать не нужно: объект создаётся сам при первом переходе,
// живёт между сценами и берёт кадры из Assets/Resources/anim_perechod.
//
// Как работает: кадры проигрываются вперёд (экран закрывается),
// загружается новая сцена, кадры играют назад (экран открывается).
//
// Использование из кода:  SceneTransition.Load("02_HubGarden");
public class SceneTransition : MonoBehaviour
{
    public const string FramesFolder = "anim_perechod"; // папка внутри Resources

    [Header("Затухание старой сцены")]
    public float fadeInTime = 2f;         // за сколько секунд гаснет текущая сцена

    [Header("Скорость")]
    public float frameDuration = 0.12f;   // сколько держится один кадр
    public float holdBetween = 0.15f;     // пауза на закрытом экране

    [Header("Как исчезает после загрузки")]
    public bool playBackwards = false;    // проигрывать кадры назад (выкл — просто растворяется)
    public float fadeOutTime = 0.35f;     // плавное исчезновение последнего кадра

    private static SceneTransition instance;
    private Sprite[] frames;
    private Image image;
    private bool busy;

    // Главная точка входа: перейти в сцену с анимацией
    public static void Load(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        var t = Get();
        if (t == null || t.frames == null || t.frames.Length == 0)
        {
            SceneManager.LoadScene(sceneName); // кадров нет — просто грузим
            return;
        }
        if (t.busy) return;
        t.StartCoroutine(t.Play(sceneName));
    }

    private static SceneTransition Get()
    {
        if (instance != null) return instance;

        var go = new GameObject("~SceneTransition");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<SceneTransition>();
        instance.Build();
        return instance;
    }

    // Собираем оверлей: канвас поверх всего + картинка на весь экран
    private void Build()
    {
        frames = LoadFrames();

        var canvasGO = new GameObject("TransitionCanvas");
        canvasGO.transform.SetParent(transform, false);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // поверх любого интерфейса

        // Пиксель-в-пиксель: размер картинки считаем сами, без искажений
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = 1f;

        var imageGO = new GameObject("TransitionImage");
        imageGO.transform.SetParent(canvasGO.transform, false);

        image = imageGO.AddComponent<Image>();
        image.raycastTarget = false;
        image.preserveAspect = false; // пропорции держим вручную (см. FitCover)

        // Картинка по центру, размер задаём кодом
        var rt = image.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        image.enabled = false;
    }

    // Кадры из Resources, отсортированные по номеру в имени
    private static Sprite[] LoadFrames()
    {
        var loaded = Resources.LoadAll<Sprite>(FramesFolder);
        if (loaded == null || loaded.Length == 0)
        {
            Debug.LogWarning($"SceneTransition: кадры не найдены в Resources/{FramesFolder}");
            return new Sprite[0];
        }

        return loaded.OrderBy(s => FrameNumber(s.name)).ToArray();
    }

    // "perechod_animation-7 16" -> 7
    private static int FrameNumber(string name)
    {
        var m = Regex.Match(name, @"-(\d+)");
        return m.Success ? int.Parse(m.Groups[1].Value) : 0;
    }

    private IEnumerator Play(string sceneName)
    {
        busy = true;
        SetAlpha(0f);
        image.enabled = true;

        // Анимация идёт одновременно с затуханием старой сцены
        var framesRoutine = StartCoroutine(PlayFrames(forward: true));
        yield return FadeIn();
        yield return framesRoutine;               // ждём, если кадры ещё идут

        var op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;

        yield return new WaitForSecondsRealtime(holdBetween);

        if (playBackwards)
            yield return PlayFrames(forward: false);
        else
            yield return FadeOut();

        image.enabled = false;
        SetAlpha(1f);
        busy = false;
    }

    private IEnumerator PlayFrames(bool forward)
    {
        for (int i = 0; i < frames.Length; i++)
        {
            int index = forward ? i : frames.Length - 1 - i;
            image.sprite = frames[index];
            FitCover(frames[index]);
            yield return new WaitForSecondsRealtime(frameDuration);
        }
    }

    // Анимация проявляется поверх текущей сцены — она «гаснет»
    private IEnumerator FadeIn()
    {
        if (fadeInTime <= 0f) { SetAlpha(1f); yield break; }

        for (float t = 0f; t < fadeInTime; t += Time.unscaledDeltaTime)
        {
            SetAlpha(t / fadeInTime);
            yield return null;
        }
        SetAlpha(1f);
    }

    // Плавно растворяем последний кадр — новая сцена проступает под ним
    private IEnumerator FadeOut()
    {
        if (fadeOutTime <= 0f) yield break;

        for (float t = 0f; t < fadeOutTime; t += Time.unscaledDeltaTime)
        {
            SetAlpha(1f - t / fadeOutTime);
            yield return null;
        }
        SetAlpha(0f);
    }

    private void SetAlpha(float a)
    {
        if (image == null) return;
        var c = image.color;
        c.a = a;
        image.color = c;
    }

    // Размер кадра: заполнить экран без искажений (лишнее обрезается по краям)
    private void FitCover(Sprite s)
    {
        if (s == null || image == null) return;

        float screenAspect = (float)Screen.width / Screen.height;
        float spriteAspect = s.rect.width / s.rect.height;

        float w, h;
        if (spriteAspect > screenAspect)
        {
            h = Screen.height;              // упираемся в высоту, бока обрезаются
            w = h * spriteAspect;
        }
        else
        {
            w = Screen.width;               // упираемся в ширину, верх/низ обрезаются
            h = w / spriteAspect;
        }

        image.rectTransform.sizeDelta = new Vector2(w, h);
    }
}
