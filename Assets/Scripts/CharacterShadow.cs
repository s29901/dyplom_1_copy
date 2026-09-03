using UnityEngine;

// Тень под персонажем: лежит у ног и всегда рисуется под ним.
//
// Повесить на ДОЧЕРНИЙ объект героя со SpriteRenderer (спрайт cien_1).
// Порядок отрисовки подстраивается автоматически, потому что у героя
// он пересчитывается каждый кадр по глубине сцены.
[RequireComponent(typeof(SpriteRenderer))]
public class CharacterShadow : MonoBehaviour
{
    [Header("Кого затеняем (пусто = родитель)")]
    public SpriteRenderer target;

    [Header("Вид")]
    [Range(0f, 1f)] public float alpha = 0.45f;
    [Tooltip("На сколько тень рисуется ниже персонажа")]
    public int sortingOffset = -1;

    [Header("Наклон тени")]
    public bool keepAngle = true;
    public float angleX = -45f;     // как у тени под деревом

    [Header("Живость")]
    [Tooltip("Тень слегка сжимается, когда персонаж идёт")]
    public bool squashOnMove = true;
    [Range(0f, 0.4f)] public float squashAmount = 0.12f;
    public float squashSpeed = 8f;   // как быстро пульсирует при ходьбе

    private SpriteRenderer sr;
    private Vector3 baseScale;
    private Vector3 lastTargetPos;
    private float phase;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // Ищем персонажа среди РОДИТЕЛЕЙ, не считая саму тень
        if (target == null && transform.parent != null)
            target = transform.parent.GetComponentInParent<SpriteRenderer>();

        if (target == null || target == sr)
            Debug.LogWarning("CharacterShadow: не найден SpriteRenderer персонажа — " +
                             "тень должна быть дочерним объектом героя, либо задай Target вручную", this);

        baseScale = transform.localScale;
        lastTargetPos = transform.position;

        var c = sr.color;
        c.a = alpha;
        sr.color = c;

        if (keepAngle)
            transform.localRotation = Quaternion.Euler(angleX, transform.localEulerAngles.y, 0f);
    }

    void LateUpdate()
    {
        // Всегда под персонажем: его порядок меняется каждый кадр
        if (target != null && target != sr)
        {
            sr.sortingLayerID = target.sortingLayerID;
            sr.sortingOrder = target.sortingOrder + sortingOffset;
        }

        if (!squashOnMove) return;

        // Пока персонаж двигается, тень чуть-чуть «дышит»
        float speed = (transform.position - lastTargetPos).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        lastTargetPos = transform.position;

        if (speed > 0.05f)
        {
            phase += Time.deltaTime * squashSpeed;
            float k = 1f + Mathf.Sin(phase) * squashAmount;
            transform.localScale = new Vector3(baseScale.x * k, baseScale.y / Mathf.Max(k, 0.01f), baseScale.z);
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, baseScale, Time.deltaTime * 6f);
        }
    }
}
