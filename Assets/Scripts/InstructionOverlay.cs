using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

// Показывает короткий видеоролик-инструкцию поверх сцены.
//
// Повесить на пустой объект в сцене, выбрать клип из Sprites/Instruction
// и момент показа. Никакой UI собирать не нужно — канвас, затемнение
// и картинка создаются скриптом при первом показе.
//
// Ролик можно запустить и вручную: метод Play() вешается на кнопку
// или вызывается из другого скрипта.
public class InstructionOverlay : MonoBehaviour
{
    public enum Trigger
    {
        Manual,         // только по вызову Play()
        OnSceneStart,   // сразу после загрузки сцены
        AfterCutscene,  // когда доиграет вступительная катсцена
        AfterBirdTalk,  // когда закончится разговор с птицей (для хаба)
        AfterIdle       // когда игрок несколько секунд ничего не делает
    }

    [Header("Ролик")]
    public VideoClip clip;

    [Header("Когда показывать")]
    public Trigger playWhen = Trigger.AfterCutscene;
    [Tooltip("Пауза перед показом, секунды")]
    public float delay = 0.5f;

    [Tooltip("Сколько раз проиграть ролик подряд, без паузы между повторами")]
    [Min(1)] public int repeatCount = 1;

    [Tooltip("Показать только после того, как отыграет другая подсказка. " +
             "Так задаётся порядок, когда в сцене их несколько.")]
    public InstructionOverlay playAfter;

    [Tooltip("Сколько секунд бездействия ждать (для варианта After Idle)")]
    public float idleSeconds = 5f;

    [Tooltip("Начинать отсчёт бездействия только после вступительной катсцены. " +
             "Иначе подсказка может всплыть посреди разговора с птицей.")]
    public bool waitForCutsceneFirst = true;

    public enum IdleSource
    {
        AnyInput,          // любой ввод с клавиатуры и мыши
        QuestInteraction,  // игрок тянет солнце или облако (сколько их — неважно)
        WatchedObject      // перемещение конкретного объекта из поля ниже
    }

    [Tooltip("Что считать действием игрока и сбрасывать отсчёт.\n" +
             "Any Input — любой ввод.\n" +
             "Quest Interaction — только перетаскивание квестового предмета. " +
             "Подходит там, где предметов много или они движутся сами, как облака в Q2.\n" +
             "Watched Object — перемещение объекта из поля Watch Movement Of.")]
    public IdleSource idleCountsFrom = IdleSource.AnyInput;

    [Tooltip("Только для варианта Watched Object")]
    public Transform watchMovementOf;

    [Header("На каком этапе игры показывать")]
    [Tooltip("Минимум пройденных квестов. 0 — с самого начала.")]
    [Range(0, 4)] public int minQuestsCompleted = 0;

    [Tooltip("Максимум пройденных квестов. 4 — до самого конца.\n" +
             "Подсказка в начале игры: 0 и 0. Финальная: 4 и 4.")]
    [Range(0, 4)] public int maxQuestsCompleted = 4;

    [Tooltip("Тот же ключ, что в Dialogue Id у BirdInteraction")]
    public string birdDialogueId = "hub_bird_intro";

    [Tooltip("Показывать только до разговора с птицей. " +
             "После разговора подсказка больше не всплывёт.")]
    public bool onlyBeforeBirdTalk = false;

    [Header("Повторы")]
    [Tooltip("Показать один раз за прохождение. Сбрасывается при выходе в главное меню.")]
    public bool onlyOncePerPlaythrough = true;
    [Tooltip("Ключ показа. Пусто — возьмётся имя клипа.")]
    public string id = "";

    public enum Placement { Fullscreen, TopRight, TopLeft, BottomRight, BottomLeft, Center }

    [Header("Где показывать")]
    public Placement placement = Placement.TopRight;

    [Tooltip("Ширина ролика как доля ширины экрана (0.3 = треть экрана). " +
             "Для Fullscreen не используется.")]
    [Range(0.1f, 1f)] public float sizePercent = 0.3f;

    [Tooltip("Отступ от края экрана в пикселях макета 3840x2160")]
    public Vector2 margin = new Vector2(60f, 60f);

    [Header("Подсветка под роликом")]
    [Tooltip("Мягкая плашка за видео, чтобы подсказка читалась на любом фоне")]
    public bool showBackdrop = true;

    [Tooltip("Спрайт плашки. Пусто — возьмётся встроенная панель со скруглением. " +
             "Можно подставить glow_warm.")]
    public Sprite backdropSprite;

    public Color backdropColor = new Color(1f, 0.96f, 0.82f, 0.35f);

    [Tooltip("Насколько плашка выступает за края ролика, в единицах макета")]
    public float backdropPadding = 40f;

    [Tooltip("Плашка мягко дышит, чтобы притянуть взгляд")]
    public bool backdropPulse = true;
    public float pulseSpeed = 1.2f;
    [Range(0f, 0.6f)] public float pulseStrength = 0.3f;

    [Header("Поведение")]
    [Tooltip("Остановить игру на время ролика. Для подсказки в углу лучше выключить.")]
    public bool pauseGame = false;
    [Tooltip("Пропустить по клику или пробелу")]
    public bool clickToSkip = false;
    [Tooltip("Затемнение сцены под роликом (0 — без затемнения)")]
    [Range(0f, 1f)] public float dimBackground = 0f;
    public float fadeDuration = 0.3f;
    [Tooltip("Ролики с озвучкой — включить звук")]
    public bool playAudio = false;

    // Показанные ролики за текущее прохождение.
    // Живёт в памяти, поэтому обнуляется вместе с выходом в главное меню.
    private static readonly HashSet<string> shown = new HashSet<string>();

    public static void ForgetShown() => shown.Clear();

    public bool IsPlaying { get; private set; }

    // Ролик отыграл до конца хотя бы раз — по этому ждут те, кто идёт следом
    public bool HasFinished { get; private set; }

    private string Key => string.IsNullOrEmpty(id)
        ? (clip != null ? clip.name : name)
        : id;

    private IEnumerator Start()
    {
        if (playWhen == Trigger.Manual) yield break;

        // Очередь между подсказками задаётся явно, а не тем,
        // чья корутина случайно оказалась первой
        if (playAfter != null && playAfter != this)
            while (!playAfter.HasFinished) yield return null;

        if (playWhen == Trigger.AfterCutscene)
        {
            yield return WaitIntro();
        }
        else if (playWhen == Trigger.AfterBirdTalk)
        {
            // Разговор помечается пройденным при открытии панели,
            // поэтому ждём ещё и закрытия диалога
            while (!BirdInteraction.WasPlayed(birdDialogueId))
                yield return null;

            var dm = DialogueManager.Instance;
            while (dm != null && dm.IsDialogueActive)
                yield return null;
        }

        else if (playWhen == Trigger.AfterIdle)
        {
            if (waitForCutsceneFirst) yield return WaitIntro();
            yield return WatchIdle();
            yield break;
        }

        if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
        Play();
    }

    // Ждёт конца вступительной катсцены. В Q1 своя реализация (Q1Cutscene),
    // в остальных квестовых сценах общая (QuestCutscene). В хабе катсцены
    // нет вовсе — тогда ожидание пропускается.
    private IEnumerator WaitIntro()
    {
        var quest = FindFirstObjectByType<QuestCutscene>();
        if (quest != null)
        {
            while (!quest.IntroFinished) yield return null;
            yield break;
        }

        var q1 = FindFirstObjectByType<Q1Cutscene>();
        if (q1 != null)
            while (!Q1Cutscene.IntroFinished) yield return null;
    }

    // Ждёт, пока игрок перестанет что-либо делать, и показывает подсказку.
    // Пока разговора с птицей не было, подсказка может всплыть снова
    // после каждой новой паузы.
    private IEnumerator WatchIdle()
    {
        float idle = 0f;
        float check = 0f;
        Vector3 lastMouse = Input.mousePosition;
        Vector3 lastWatched = watchMovementOf != null ? watchMovementOf.position : Vector3.zero;

        while (true)
        {
            check -= Time.unscaledDeltaTime;
            if (check <= 0f)
            {
                check = 0.25f;

                // Разговор состоялся — подсказка отработала своё
                if (onlyBeforeBirdTalk && BirdInteraction.WasPlayed(birdDialogueId))
                    yield break;

                // Этап игры ушёл вперёд — подсказывать больше нечего.
                // Раньше срока не выходим: ждём своего окна.
                var pm = ProgressManager.Instance;
                int done = pm != null ? pm.QuestsCompleted() : 0;
                if (done > maxQuestsCompleted) yield break;
                if (done < minQuestsCompleted) { idle = 0f; yield return null; continue; }
            }

            bool busy = IsPlaying || DialogueActive;

            // Раньше режим выбирался просто наличием объекта в Watch Movement Of.
            // Если поле заполнено, а список остался в положении по умолчанию —
            // это старая настройка, и её надо понимать по-старому.
            IdleSource source = idleCountsFrom;
            if (source == IdleSource.AnyInput && watchMovementOf != null)
                source = IdleSource.WatchedObject;

            bool acted;
            switch (source)
            {
                case IdleSource.QuestInteraction:
                    // Общий сигнал от солнца и облаков. Их может быть
                    // сколько угодно, и собственный дрейф сигнала не даёт
                    acted = PlayerActivity.SecondsSinceTouch < 0.2f;
                    break;

                case IdleSource.WatchedObject:
                    acted = watchMovementOf != null &&
                            (watchMovementOf.position - lastWatched).sqrMagnitude > 0.0001f;
                    if (watchMovementOf != null) lastWatched = watchMovementOf.position;
                    break;

                default:
                    acted = (Input.mousePosition - lastMouse).sqrMagnitude > 4f ||
                            Input.anyKey ||
                            Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f ||
                            Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f;
                    lastMouse = Input.mousePosition;
                    break;
            }

            if (busy || acted)
                idle = 0f;
            else
                idle += Time.unscaledDeltaTime;

            if (idle >= idleSeconds)
            {
                idle = 0f;
                Play();

                // Ждём конца показа, иначе таймер побежит поверх ролика
                while (IsPlaying) yield return null;

                if (onlyOncePerPlaythrough && shown.Contains(Key)) yield break;
            }

            yield return null;
        }
    }

    // Подходит ли текущий этап игры под окно показа
    private bool StageMatches()
    {
        var pm = ProgressManager.Instance;
        int done = pm != null ? pm.QuestsCompleted() : 0;
        return done >= minQuestsCompleted && done <= maxQuestsCompleted;
    }

    // Можно вешать на кнопку или вызывать из другого скрипта
    public void Play()
    {
        if (IsPlaying || clip == null) return;
        if (onlyOncePerPlaythrough && shown.Contains(Key)) return;
        if (onlyBeforeBirdTalk && BirdInteraction.WasPlayed(birdDialogueId)) return;
        if (!StageMatches()) return;

        shown.Add(Key);
        StartCoroutine(Show());
    }

    // Два ролика одновременно выглядели бы кашей — второй ждёт своей очереди
    private static bool anyPlaying;

    // Если объект выключат или сцена сменится посреди ролика, корутина
    // оборвётся и timeScale останется нулевым — игра встанет намертво.
    private bool paused;
    private float pausedScale = 1f;

    private void OnDisable()
    {
        if (paused)
        {
            Time.timeScale = pausedScale;
            paused = false;
        }

        if (IsPlaying)
        {
            IsPlaying = false;
            anyPlaying = false;
        }
    }

    private static bool DialogueActive =>
        DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive;

    private IEnumerator Show()
    {
        // Ждём и своей очереди, и конца любого разговора: подсказка
        // не должна выскакивать поверх реплик птицы
        while (anyPlaying || DialogueActive) yield return null;

        // Пока ждали, условия могли измениться — проверяем заново
        if (onlyBeforeBirdTalk && BirdInteraction.WasPlayed(birdDialogueId)) yield break;
        if (!StageMatches()) yield break;

        anyPlaying = true;
        IsPlaying = true;

        var ui = BuildOverlay();
        var player = ui.player;

        player.clip = clip;
        player.Prepare();
        while (!player.isPrepared) yield return null;

        ui.image.texture = player.targetTexture;

        // Пауза имеет смысл только для полноэкранной вставки. Подсказка в углу
        // не должна останавливать игру, даже если галочку забыли снять.
        bool willPause = pauseGame && placement == Placement.Fullscreen;

        float savedScale = Time.timeScale;
        if (willPause)
        {
            pausedScale = savedScale;
            paused = true;
            Time.timeScale = 0f;
        }

        yield return Fade(ui, 0f, 1f);

        // Повторы делаем зацикливанием: между проходами не будет паузы,
        // в отличие от повторного Play() после остановки
        int loopsLeft = Mathf.Max(1, repeatCount) - 1;
        bool finished = false;

        player.isLooping = loopsLeft > 0;
        player.loopPointReached += _ =>
        {
            if (loopsLeft > 0)
            {
                loopsLeft--;
                if (loopsLeft == 0) player.isLooping = false;
            }
            else finished = true;
        };

        player.Play();

        // Ролик идёт по своим часам и не зависит от Time.timeScale
        yield return null;
        while (!finished)
        {
            if (clickToSkip && (Input.GetMouseButtonDown(0) ||
                                Input.GetKeyDown(KeyCode.Space) ||
                                Input.GetKeyDown(KeyCode.Escape)))
                break;
            yield return null;
        }

        player.Stop();
        yield return Fade(ui, 1f, 0f);

        if (willPause)
        {
            Time.timeScale = savedScale;
            paused = false;
        }

        if (player.targetTexture != null)
        {
            player.targetTexture.Release();
            Destroy(player.targetTexture);
        }
        Destroy(ui.root);

        IsPlaying = false;
        anyPlaying = false;
        HasFinished = true;
    }

    private IEnumerator Fade(Overlay ui, float from, float to)
    {
        if (fadeDuration <= 0f)
        {
            ui.group.alpha = to;
            yield break;
        }

        for (float t = 0f; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            ui.group.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        ui.group.alpha = to;
    }

    // ---------- сборка интерфейса ----------

    private struct Overlay
    {
        public GameObject root;
        public CanvasGroup group;
        public RawImage image;
        public VideoPlayer player;
    }

    private Overlay BuildOverlay()
    {
        var root = new GameObject("InstructionOverlay");

        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;          // выше меню и диалогов

        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(3840, 2160);
        scaler.matchWidthOrHeight = 0.5f;

        var group = root.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;       // клики ловим сами, через Input

        // Затемнение сцены
        if (dimBackground > 0f)
        {
            var dim = new GameObject("Dim", typeof(Image));
            dim.transform.SetParent(root.transform, false);
            var dimImage = dim.GetComponent<Image>();
            dimImage.color = new Color(0f, 0f, 0f, dimBackground);
            dimImage.raycastTarget = false;
            Stretch(dim.GetComponent<RectTransform>());
        }

        // Контейнер держит положение и размер, внутри — подсветка и само видео
        var holder = new GameObject("Video", typeof(RectTransform), typeof(AspectRatioFitter));
        holder.transform.SetParent(root.transform, false);

        var fitter = holder.GetComponent<AspectRatioFitter>();
        fitter.aspectRatio = clip.height > 0 ? (float)clip.width / clip.height : 16f / 9f;
        PlaceVideo(holder.GetComponent<RectTransform>(), fitter);

        // Подсветка — первым ребёнком, чтобы рисоваться под видео
        if (showBackdrop)
            BuildBackdrop(holder.transform);

        var video = new GameObject("Frame", typeof(RawImage));
        video.transform.SetParent(holder.transform, false);

        var image = video.GetComponent<RawImage>();
        image.raycastTarget = false;
        Stretch(video.GetComponent<RectTransform>());

        // Проигрыватель
        var player = root.AddComponent<VideoPlayer>();
        player.playOnAwake = false;
        player.isLooping = false;
        player.renderMode = VideoRenderMode.RenderTexture;
        var rt = new RenderTexture((int)clip.width, (int)clip.height, 0,
                                   RenderTextureFormat.ARGB32);
        rt.Create();

        // Свежая RenderTexture заполнена мусором, а первый кадр приходит
        // не мгновенно — гасим её в прозрачный, иначе мелькнёт чёрный прямоугольник
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = prev;

        player.targetTexture = rt;
        player.audioOutputMode = playAudio ? VideoAudioOutputMode.Direct
                                           : VideoAudioOutputMode.None;
        if (playAudio)
            player.SetDirectAudioVolume(0, AudioManager.Instance != null
                ? AudioManager.Instance.sfxVolume : 1f);

        DontDestroyOnLoad(root);   // ролик доиграет, даже если сцена сменится

        return new Overlay { root = root, group = group, image = image, player = player };
    }

    private void BuildBackdrop(Transform parent)
    {
        var back = new GameObject("Backdrop", typeof(Image));
        back.transform.SetParent(parent, false);

        var img = back.GetComponent<Image>();
        img.sprite = backdropSprite != null
            ? backdropSprite
            : Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        // Скругление тянется только у спрайтов с рамкой (border)
        img.type = (img.sprite != null && img.sprite.border.sqrMagnitude > 0f)
            ? Image.Type.Sliced
            : Image.Type.Simple;
        img.color = backdropColor;
        img.raycastTarget = false;

        // Плашка больше ролика на backdropPadding с каждой стороны
        var rt = back.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(-backdropPadding, -backdropPadding);
        rt.offsetMax = new Vector2(backdropPadding, backdropPadding);

        if (!backdropPulse) return;

        var pulse = back.AddComponent<UIGlowPulse>();
        pulse.maxAlpha = backdropColor.a;
        pulse.minAlpha = backdropColor.a * (1f - pulseStrength);
        pulse.speed = pulseSpeed;
        pulse.pulseScale = false;   // размер держит контейнер
        pulse.randomPhase = false;
    }

    // Раскладка ролика: на весь экран или прижатым к углу.
    // Размеры считаются в единицах макета 3840x2160 — CanvasScaler
    // пересчитает их под реальное разрешение.
    private void PlaceVideo(RectTransform rt, AspectRatioFitter fitter)
    {
        if (placement == Placement.Fullscreen)
        {
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            Stretch(rt);
            return;
        }

        // Высоту задаёт fitter по пропорциям клипа
        fitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;

        float width = 3840f * Mathf.Clamp01(sizePercent);
        rt.sizeDelta = new Vector2(width, 0f);

        Vector2 corner;   // он же pivot: угол прямоугольника прижимаем к тому же углу экрана
        Vector2 offset;

        switch (placement)
        {
            case Placement.TopRight:
                corner = new Vector2(1f, 1f);
                offset = new Vector2(-margin.x, -margin.y);
                break;
            case Placement.TopLeft:
                corner = new Vector2(0f, 1f);
                offset = new Vector2(margin.x, -margin.y);
                break;
            case Placement.BottomRight:
                corner = new Vector2(1f, 0f);
                offset = new Vector2(-margin.x, margin.y);
                break;
            case Placement.BottomLeft:
                corner = new Vector2(0f, 0f);
                offset = new Vector2(margin.x, margin.y);
                break;
            default:  // Center
                corner = new Vector2(0.5f, 0.5f);
                offset = Vector2.zero;
                break;
        }

        rt.anchorMin = rt.anchorMax = rt.pivot = corner;
        rt.anchoredPosition = offset;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
