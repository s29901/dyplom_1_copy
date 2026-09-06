using UnityEngine;

// Тень под зверьком-чувством из Q3 (и под любым другим ходячим спрайтом).
//
// Повесить на ДОЧЕРНИЙ объект зверька со SpriteRenderer (спрайт cien_tree).
// Порядок отрисовки берётся у самого зверька, поэтому тень остаётся под ним,
// даже когда DepthSort пересчитывает сортировку каждый кадр.
//
// Главное отличие от тени дерева: зверёк подпрыгивает, а тень должна
// остаться лежать на земле и слегка поджаться, пока он в воздухе.
[RequireComponent(typeof(SpriteRenderer))]
public class AnimalShadow : MonoBehaviour
{
    [Header("Кого затеняем (пусто = родитель)")]
    public SpriteRenderer target;

    [Header("Вид")]
    [Range(0f, 1f)] public float alpha = 0.4f;
    [Tooltip("На сколько тень рисуется ниже зверька")]
    public int sortingOffset = -1;

    [Header("Наклон тени")]
    public bool keepAngle = true;

    // 90 — спрайт лежит в плоскости земли (камера наклонена на 45°,
    // поэтому лежачая тень читается как эллипс)
    public float angleX = 90f;

    [Header("Прыжок")]
    [Tooltip("Тень остаётся на земле, пока зверёк подскакивает")]
    public bool stayOnGround = true;

    [Tooltip("Насколько тень уменьшается в высшей точке прыжка")]
    [Range(0f, 0.8f)] public float shrinkInAir = 0.35f;

    [Tooltip("Высота прыжка зверька — по ней считается усадка. " +
             "Должна совпадать с Hop Height в EmotionAnimal.")]
    public float hopHeight = 0.25f;

    private SpriteRenderer sr;
    private Transform owner;
    private Vector3 baseLocalPos;
    private Vector3 baseScale;
    private float groundY;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        owner = transform.parent;

        if (target == null && owner != null)
            target = owner.GetComponent<SpriteRenderer>();
        if (target == null && owner != null)
            target = owner.GetComponentInParent<SpriteRenderer>();

        if (target == null || target == sr)
            Debug.LogWarning("AnimalShadow: не найден SpriteRenderer зверька — " +
                             "тень должна быть дочерним объектом, либо задай Target вручную", this);

        baseLocalPos = transform.localPosition;
        baseScale = transform.localScale;
        groundY = owner != null ? owner.position.y : transform.position.y;

        var c = sr.color;
        c.a = alpha;
        sr.color = c;

        if (keepAngle)
            transform.localRotation = Quaternion.Euler(angleX, transform.localEulerAngles.y, 0f);
    }

    void LateUpdate()
    {
        // Всегда под зверьком: его порядок пересчитывается каждый кадр
        if (target != null && target != sr)
        {
            sr.sortingLayerID = target.sortingLayerID;
            sr.sortingOrder = target.sortingOrder + sortingOffset;
        }

        if (!stayOnGround || owner == null) return;

        float lift = owner.position.y - groundY;

        // Зверька переставили в сцене или он сменил высоту насовсем —
        // считаем новую высоту землёй, иначе тень осталась бы висеть отдельно
        if (Mathf.Abs(lift) > Mathf.Max(hopHeight, 0.01f) * 3f)
        {
            groundY = owner.position.y;
            lift = 0f;
        }

        // Компенсируем подъём родителя, чтобы тень осталась на земле
        float scaleY = owner.lossyScale.y;
        float localLift = Mathf.Abs(scaleY) > 0.0001f ? lift / scaleY : lift;

        var lp = baseLocalPos;
        lp.y = baseLocalPos.y - localLift;
        transform.localPosition = lp;

        // В воздухе тень чуть меньше
        float k = 1f;
        if (hopHeight > 0.0001f)
            k = 1f - Mathf.Clamp01(lift / hopHeight) * shrinkInAir;

        transform.localScale = baseScale * k;
    }
}
