using UnityEngine;

// Пульсация свечения: ореол мягко «дышит» — меняет прозрачность и размер.
// Повесить на объект со SpriteRenderer (например, на Glow с glow_warm).
//
// Работает и в редакторе, если включить Preview In Editor —
// удобно подбирать параметры, не запуская игру.
[ExecuteAlways]
public class GlowPulse : MonoBehaviour
{
    [Header("Прозрачность")]
    [Range(0f, 1f)] public float minAlpha = 0.35f;
    [Range(0f, 1f)] public float maxAlpha = 0.75f;

    [Header("Размер")]
    public bool pulseScale = true;
    [Range(0f, 0.5f)] public float scaleAmount = 0.08f; // насколько раздувается

    [Header("Ритм")]
    public float speed = 1.2f;          // циклов в секунду (примерно)
    public bool randomPhase = true;     // чтобы несколько ореолов не мигали в такт

    [Header("Мерцание (лёгкая неровность, как у живого огня)")]
    public bool flicker = false;
    [Range(0f, 0.3f)] public float flickerAmount = 0.08f;

    [Header("Редактор")]
    public bool previewInEditor = true;

    private SpriteRenderer sr;
    private Vector3 baseScale;
    private float phase;
    private float noiseSeed;

    void OnEnable()
    {
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        phase = randomPhase ? Random.Range(0f, Mathf.PI * 2f) : 0f;
        noiseSeed = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (!Application.isPlaying && !previewInEditor) return;
        if (sr == null) return;

        // Плавная волна от 0 до 1
        float t = (Mathf.Sin(Time.time * speed + phase) + 1f) * 0.5f;

        // Неровность огня поверх ровной волны
        if (flicker)
        {
            float n = Mathf.PerlinNoise(noiseSeed, Time.time * speed * 2f) - 0.5f;
            t = Mathf.Clamp01(t + n * flickerAmount * 2f);
        }

        var c = sr.color;
        c.a = Mathf.Lerp(minAlpha, maxAlpha, t);
        sr.color = c;

        if (pulseScale)
            transform.localScale = baseScale * (1f + (t - 0.5f) * 2f * scaleAmount);
    }

    void OnDisable()
    {
        // Возвращаем исходный размер, чтобы объект не «застыл» раздутым
        if (baseScale != Vector3.zero)
            transform.localScale = baseScale;
    }
}
