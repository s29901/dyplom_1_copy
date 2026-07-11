using System.Collections;
using UnityEngine;

// Показывает стадию роста дерева по глобальному прогрессу (ProgressManager).
// Стадия общая для всей игры, поэтому дерево с этим скриптом
// в любой сцене всегда выглядит так же, как в хабе.
// При смене стадии проигрывает "магический" переход:
// растворение -> новый спрайт -> проявление + частицы.
public class PlantGrowth : MonoBehaviour
{
    // 5 спрайтов в Inspector: от ростка до дерева
    [SerializeField] private Sprite[] growthStages = new Sprite[5];

    [Header("Магический эффект")]
    [SerializeField] private ParticleSystem magicParticles; // необязательно: искры в момент смены
    [SerializeField] private float fadeDuration = 0.4f;     // время растворения/проявления

    private SpriteRenderer spriteRenderer;
    private int lastStage = -1;
    private bool transitioning;

    // Идёт ли сейчас магическое превращение (для квестов, ждущих его конца)
    public bool IsTransitioning => transitioning;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Первый показ — мгновенно, без эффекта
        int stage = CurrentStage();
        if (stage >= 0)
        {
            lastStage = stage;
            if (growthStages[stage] != null)
                spriteRenderer.sprite = growthStages[stage];
        }
    }

    void Update()
    {
        UpdatePlant();
    }

    // Публичный метод для совместимости: его вызывают квест-менеджеры.
    // Проверяет стадию и при изменении запускает магический переход.
    public void UpdatePlant()
    {
        if (transitioning) return;

        int stage = CurrentStage();
        if (stage < 0 || stage == lastStage) return;

        lastStage = stage;
        if (growthStages[stage] != null)
            StartCoroutine(MagicTransition(growthStages[stage]));
    }

    private int CurrentStage()
    {
        if (ProgressManager.Instance == null) return -1;
        int stage = ProgressManager.Instance.QuestsCompleted();
        return Mathf.Clamp(stage, 0, growthStages.Length - 1);
    }

    private IEnumerator MagicTransition(Sprite newSprite)
    {
        transitioning = true;

        // Искры в момент превращения
        if (magicParticles != null)
            magicParticles.Play();

        // Плавное растворение старого спрайта
        yield return Fade(1f, 0f);

        spriteRenderer.sprite = newSprite;

        // Плавное проявление нового
        yield return Fade(0f, 1f);

        transitioning = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        Color c = spriteRenderer.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / fadeDuration);
            spriteRenderer.color = c;
            yield return null;
        }
        c.a = to;
        spriteRenderer.color = c;
    }
}
