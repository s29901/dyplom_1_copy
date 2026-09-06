using UnityEngine;
using UnityEngine.UI;

// Пульсация свечения для UI — то же, что GlowPulse, но для Image внутри Canvas.
// Обычный GlowPulse работает со SpriteRenderer и на кнопках не заводится.
//
// Повесить на дочерний Image с ореолом (спрайт glow_warm).
// Чтобы ореол оказался ПОД кнопкой, объект должен стоять
// выше неё в иерархии — порядок отрисовки UI идёт сверху вниз.
[ExecuteAlways]
[RequireComponent(typeof(Graphic))]
public class UIGlowPulse : MonoBehaviour
{
    [Header("Прозрачность")]
    [Range(0f, 1f)] public float minAlpha = 0.35f;
    [Range(0f, 1f)] public float maxAlpha = 0.75f;

    [Header("Размер")]
    public bool pulseScale = true;
    [Range(0f, 0.5f)] public float scaleAmount = 0.08f;

    [Header("Ритм")]
    public float speed = 1.2f;         // примерно циклов в секунду
    public bool randomPhase = true;

    [Header("Мерцание (лёгкая неровность, как у живого огня)")]
    public bool flicker = false;
    [Range(0f, 0.3f)] public float flickerAmount = 0.08f;

    [Header("Редактор")]
    public bool previewInEditor = true;

    private Graphic graphic;
    private Vector3 baseScale;
    private float phase;
    private float noiseSeed;

    void OnEnable()
    {
        graphic = GetComponent<Graphic>();
        baseScale = transform.localScale;
        phase = randomPhase ? Random.Range(0f, Mathf.PI * 2f) : 0f;
        noiseSeed = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (!Application.isPlaying && !previewInEditor) return;
        if (graphic == null) return;

        // Плавная волна от 0 до 1
        float t = (Mathf.Sin(Time.unscaledTime * speed + phase) + 1f) * 0.5f;

        if (flicker)
        {
            float n = Mathf.PerlinNoise(noiseSeed, Time.unscaledTime * speed * 2f) - 0.5f;
            t = Mathf.Clamp01(t + n * flickerAmount * 2f);
        }

        var c = graphic.color;
        c.a = Mathf.Lerp(minAlpha, maxAlpha, t);
        graphic.color = c;

        if (pulseScale)
            transform.localScale = baseScale * (1f + (t - 0.5f) * 2f * scaleAmount);
    }

    void OnDisable()
    {
        // Возвращаем исходный размер, чтобы ореол не застыл раздутым
        if (baseScale != Vector3.zero)
            transform.localScale = baseScale;
    }
}
