using System.Collections;
using UnityEngine;

// Показывает стадию роста дерева по глобальному прогрессу (ProgressManager).
// Стадия общая для всей игры, поэтому дерево с этим скриптом
// в любой сцене всегда выглядит так же, как в хабе.
//
// У каждой стадии свой набор кадров (например tree_2_1 / tree_2_2 / tree_2_3) —
// они бесконечно перебираются, и дерево «дышит», пока стоит в этой стадии.
// При смене стадии играет "магический" переход: растворение -> новые кадры -> проявление.
public class PlantGrowth : MonoBehaviour
{
    [System.Serializable]
    public class GrowthStage
    {
        [Tooltip("Кадры анимации этой стадии (1-3 шт). Перебираются по кругу.")]
        public Sprite[] frames;

        [Tooltip("Сколько секунд держится один кадр")]
        public float frameDuration = 0.6f;

        public bool HasFrames => frames != null && frames.Length > 0 && frames[0] != null;
    }

    [Header("Стадии: 0 = росток ... 4 = взрослое дерево")]
    [SerializeField] private GrowthStage[] stages = new GrowthStage[5];

    [Header("Устаревшее: по одному спрайту на стадию (запасной вариант)")]
    [SerializeField] private Sprite[] growthStages = new Sprite[5];

    // Рисованное превращение: вихрь листьев поверх дерева.
    // Кадры лежат в Sprites/TREE_ANIM/<стадия>-<стадия>, например 1-2.
    // Кадры с "_tree1" в имени играют, пока видно СТАРОЕ дерево,
    // кадры с "_tree2" — уже после подмены. Момент подмены скрыт
    // самым плотным кадром вихря, поэтому переход не заметен.
    [System.Serializable]
    public class TransitionAnim
    {
        [Tooltip("Кадры из папки превращения. Можно перетащить всю папку разом — " +
                 "порядок скрипт восстановит сам по номеру в имени файла.")]
        public Sprite[] frames;

        [Tooltip("Кадров в секунду")]
        public float frameRate = 12f;

        [Tooltip("Сдвиг эффекта для этой стадии (если вихрь встал не по центру кроны)")]
        public Vector2 offset;

        [Tooltip("Множитель размера эффекта для этой стадии")]
        public float scale = 1f;

        public bool HasFrames => frames != null && frames.Length > 0 && frames[0] != null;
    }

    [Header("Превращение: рисованная анимация")]
    [Tooltip("Дочерний объект дерева со SpriteRenderer, выключенный по умолчанию. " +
             "Sorting Order выше дерева. Пусто — найдётся ребёнок с именем transform_effect.")]
    [SerializeField] private SpriteRenderer effectRenderer;

    [Tooltip("По одной записи на переход: 0 = из стадии 1 в 2, 1 = из 2 в 3 и так далее")]
    [SerializeField] private TransitionAnim[] transitions = new TransitionAnim[4];

    [Header("Магический эффект (запасной вариант, если кадров нет)")]
    [SerializeField] private ParticleSystem magicParticles; // искры в момент смены
    [SerializeField] private float fadeDuration = 0.4f;     // растворение/проявление

    [Header("Покачивание кадров")]
    [SerializeField] private bool randomStartFrame = true;  // чтобы деревья не мигали в такт

    private SpriteRenderer spriteRenderer;
    private int lastStage = -1;
    private bool transitioning;
    private int frameIndex;
    private float frameTimer;
    private Vector3 effectBasePos;
    private Vector3 effectBaseScale = Vector3.one;

    public bool IsTransitioning => transitioning;

    // Текущая стадия (0-4) — нужна тени и другим эффектам
    public int CurrentStageIndex => lastStage;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetupEffect();

        int stage = CurrentStage();
        if (stage >= 0)
        {
            lastStage = stage;
            ResetAnimation(stage);
            ApplyFrame(stage);
        }
    }

    void Update()
    {
        UpdatePlant();
        AnimateFrames();
    }

    // Проверяет стадию и при изменении запускает магический переход
    public void UpdatePlant()
    {
        if (transitioning) return;

        int stage = CurrentStage();
        if (stage < 0 || stage == lastStage) return;

        lastStage = stage;
        StartCoroutine(MagicTransition(stage));
    }

    // Перелистывание кадров внутри текущей стадии
    private void AnimateFrames()
    {
        if (transitioning || lastStage < 0) return;

        var st = GetStage(lastStage);
        if (st == null || st.frames == null || st.frames.Length < 2) return;

        frameTimer += Time.deltaTime;
        if (frameTimer >= st.frameDuration)
        {
            frameTimer -= st.frameDuration;
            frameIndex = (frameIndex + 1) % st.frames.Length;
            ApplyFrame(lastStage);
        }
    }

    private void ResetAnimation(int stage)
    {
        var st = GetStage(stage);
        int count = (st != null && st.frames != null) ? st.frames.Length : 1;
        frameIndex = (randomStartFrame && count > 1) ? Random.Range(0, count) : 0;
        frameTimer = 0f;
    }

    private void ApplyFrame(int stage)
    {
        if (spriteRenderer == null) return;

        var st = GetStage(stage);
        Sprite s = null;

        if (st != null && st.HasFrames)
            s = st.frames[Mathf.Clamp(frameIndex, 0, st.frames.Length - 1)];
        else if (growthStages != null && stage < growthStages.Length)
            s = growthStages[stage]; // запасной вариант: один спрайт на стадию

        if (s != null) spriteRenderer.sprite = s;
    }

    private GrowthStage GetStage(int i)
    {
        if (stages == null || i < 0 || i >= stages.Length) return null;
        var st = stages[i];
        return (st != null && st.HasFrames) ? st : null;
    }

    private int CurrentStage()
    {
        if (ProgressManager.Instance == null) return -1;
        int stage = ProgressManager.Instance.QuestsCompleted();
        int max = Mathf.Max(
            stages != null ? stages.Length : 0,
            growthStages != null ? growthStages.Length : 0) - 1;
        return Mathf.Clamp(stage, 0, Mathf.Max(max, 0));
    }

    private IEnumerator MagicTransition(int newStage)
    {
        transitioning = true;

        AudioManager.PlaySfx("tree_transform");

        var anim = GetTransition(newStage);

        if (anim != null && effectRenderer != null)
            yield return PlayTransitionAnim(anim, newStage);
        else
            yield return FadeSwap(newStage);   // запасной вариант: растворение

        transitioning = false;
    }

    // Вихрь листьев поверх дерева. Дерево подменяется в тот кадр,
    // где вихрь плотнее всего, — на первом кадре с "_tree2" в имени.
    private IEnumerator PlayTransitionAnim(TransitionAnim anim, int newStage)
    {
        Sprite[] frames = OrderedFrames(anim.frames);
        int swapAt = SwapIndex(frames);

        var tr = effectRenderer.transform;
        tr.localPosition = effectBasePos + (Vector3)anim.offset;
        tr.localScale = effectBaseScale * Mathf.Max(anim.scale, 0.0001f);
        effectRenderer.gameObject.SetActive(true);

        float step = 1f / Mathf.Max(anim.frameRate, 1f);
        bool swapped = false;

        for (int i = 0; i < frames.Length; i++)
        {
            if (!swapped && i >= swapAt)
            {
                ResetAnimation(newStage);
                ApplyFrame(newStage);
                swapped = true;
            }

            effectRenderer.sprite = frames[i];
            yield return new WaitForSeconds(step);
        }

        if (!swapped)   // на случай, если в именах нет "_tree2"
        {
            ResetAnimation(newStage);
            ApplyFrame(newStage);
        }

        effectRenderer.gameObject.SetActive(false);
        tr.localPosition = effectBasePos;
        tr.localScale = effectBaseScale;
    }

    private IEnumerator FadeSwap(int newStage)
    {
        if (magicParticles != null)
            magicParticles.Play();

        yield return Fade(1f, 0f);   // растворяем старое дерево

        ResetAnimation(newStage);
        ApplyFrame(newStage);        // подставляем кадр новой стадии

        yield return Fade(0f, 1f);   // проявляем новое
    }

    private void SetupEffect()
    {
        if (effectRenderer == null)
        {
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
                if (t != transform && t.name.Trim().ToLower() == "transform_effect")
                {
                    effectRenderer = t.GetComponent<SpriteRenderer>();
                    break;
                }
        }

        if (effectRenderer == null) return;

        effectBasePos = effectRenderer.transform.localPosition;
        effectBaseScale = effectRenderer.transform.localScale;
        effectRenderer.gameObject.SetActive(false);
    }

    private TransitionAnim GetTransition(int newStage)
    {
        int i = newStage - 1;   // переход 1->2 лежит в ячейке 0
        if (transitions == null || i < 0 || i >= transitions.Length) return null;
        var t = transitions[i];
        return (t != null && t.HasFrames) ? t : null;
    }

    // Кадры сортируются по номеру в имени: "4-5-10_tree2" -> 10.
    // Иначе Unity расставит их по алфавиту и 10 окажется перед 2.
    private static Sprite[] OrderedFrames(Sprite[] src)
    {
        var list = new System.Collections.Generic.List<Sprite>();
        foreach (var s in src) if (s != null) list.Add(s);

        list.Sort((a, b) =>
        {
            int na = FrameNumber(a.name), nb = FrameNumber(b.name);
            if (na != nb) return na.CompareTo(nb);
            return string.CompareOrdinal(a.name, b.name);
        });
        return list.ToArray();
    }

    private static int FrameNumber(string name)
    {
        int cut = name.IndexOf('_');
        string head = (cut > 0 ? name.Substring(0, cut) : name).Trim();

        int dash = head.LastIndexOf('-');
        string tail = dash >= 0 ? head.Substring(dash + 1) : head;

        return int.TryParse(tail.Trim(), out int n) ? n : int.MaxValue;
    }

    private static int SwapIndex(Sprite[] frames)
    {
        for (int i = 0; i < frames.Length; i++)
            if (frames[i].name.ToLower().Contains("tree2")) return i;

        return frames.Length / 2;   // имён нет — меняем ровно посередине
    }

    private IEnumerator Fade(float from, float to)
    {
        if (spriteRenderer == null) yield break;

        for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
        {
            var c = spriteRenderer.color;
            c.a = Mathf.Lerp(from, to, t / fadeDuration);
            spriteRenderer.color = c;
            yield return null;
        }

        var end = spriteRenderer.color;
        end.a = to;
        spriteRenderer.color = end;
    }
}
