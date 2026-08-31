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

    [Header("Магический эффект")]
    [SerializeField] private ParticleSystem magicParticles; // искры в момент смены
    [SerializeField] private float fadeDuration = 0.4f;     // растворение/проявление

    [Header("Покачивание кадров")]
    [SerializeField] private bool randomStartFrame = true;  // чтобы деревья не мигали в такт

    private SpriteRenderer spriteRenderer;
    private int lastStage = -1;
    private bool transitioning;
    private int frameIndex;
    private float frameTimer;

    public bool IsTransitioning => transitioning;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

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

        if (magicParticles != null)
            magicParticles.Play();

        yield return Fade(1f, 0f);   // растворяем старое дерево

        ResetAnimation(newStage);
        ApplyFrame(newStage);        // подставляем кадр новой стадии

        yield return Fade(0f, 1f);   // проявляем новое

        transitioning = false;
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
