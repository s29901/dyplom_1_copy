using System.Collections;
using UnityEngine;

// Тень под растением: лежит на земле и растёт вместе с деревом.
//
// Повесить на ДОЧЕРНИЙ объект дерева со SpriteRenderer.
// Поворот тени: X = 90 (лежит в плоскости земли), Sorting Order ниже дерева.
//
// Размер берётся из stageScale по номеру стадии; если заданы stageSprites —
// ещё и меняется картинка тени.
public class PlantShadow : MonoBehaviour
{
    [Header("Растение (пусто = взять из родителя)")]
    public PlantGrowth plant;

    [Header("Размер тени для каждой стадии (0-4)")]
    public float[] stageScale = { 0.6f, 0.9f, 1.2f, 1.5f, 1.8f };

    [Header("Свой спрайт тени на стадию (необязательно)")]
    public Sprite[] stageSprites;

    [Header("Вид")]
    [Range(0f, 1f)] public float alpha = 0.5f;

    [Tooltip("Принудительно выставлять наклон тени при старте. " +
             "Выключи, если хочешь задавать поворот вручную в сцене.")]
    public bool keepFlat = false;

    [Tooltip("Наклон тени по X, когда Keep Flat включён (90 = плашмя на земле)")]
    public float flatAngleX = 45f;
    public float changeDuration = 0.6f; // плавность изменения размера

    private SpriteRenderer sr;
    private int lastStage = -999;
    private Vector3 baseScale;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (plant == null) plant = GetComponentInParent<PlantGrowth>();

        baseScale = transform.localScale;

        if (keepFlat)
            transform.localRotation = Quaternion.Euler(flatAngleX, transform.localEulerAngles.y, 0f);

        if (sr != null)
        {
            var c = sr.color; c.a = alpha; sr.color = c;
        }

        Apply(CurrentStage(), instant: true);
    }

    void Update()
    {
        int stage = CurrentStage();
        if (stage != lastStage)
            Apply(stage, instant: false);
    }

    private int CurrentStage()
    {
        if (plant != null && plant.CurrentStageIndex >= 0)
            return plant.CurrentStageIndex;
        if (ProgressManager.Instance != null)
            return ProgressManager.Instance.QuestsCompleted();
        return 0;
    }

    private void Apply(int stage, bool instant)
    {
        lastStage = stage;

        // Картинка тени для этой стадии
        if (sr != null && stageSprites != null && stage >= 0 && stage < stageSprites.Length
            && stageSprites[stage] != null)
            sr.sprite = stageSprites[stage];

        // Размер
        float k = (stageScale != null && stage >= 0 && stage < stageScale.Length)
            ? stageScale[stage] : 1f;
        Vector3 target = new Vector3(baseScale.x * k, baseScale.y * k, baseScale.z);

        StopAllCoroutines();
        if (instant || changeDuration <= 0f)
            transform.localScale = target;
        else
            StartCoroutine(ScaleTo(target));
    }

    private IEnumerator ScaleTo(Vector3 target)
    {
        // Ждём, пока дерево доиграет своё превращение
        if (plant != null)
            while (plant.IsTransitioning) yield return null;

        Vector3 from = transform.localScale;
        for (float t = 0f; t < changeDuration; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(from, target, t / changeDuration);
            yield return null;
        }
        transform.localScale = target;
    }
}
