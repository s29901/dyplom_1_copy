using UnityEngine;

// Сортировка спрайтов по глубине (2.5D).
// Объект, стоящий ближе к камере (меньше Z), рисуется поверх.
//
// Повесить на кусты, деревья, камни — на всё, что стоит на земле.
// Формула совпадает с той, что использует герой, поэтому персонаж
// и декорации сортируются в одной системе координат.
[ExecuteAlways]
public class DepthSort : MonoBehaviour
{
    [Header("Пересчитывать каждый кадр (для движущихся объектов)")]
    public bool dynamic = false;

    [Header("Точка, по которой считается глубина (пусто = сам объект)")]
    [Tooltip("Обычно это основание объекта — там, где он касается земли")]
    public Transform anchor;

    [Header("Сдвиг: +1 нарисует чуть выше соседа с той же глубиной")]
    public int offset = 0;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        Apply();
    }

    void Update()
    {
        if (dynamic || !Application.isPlaying)
            Apply();
    }

    public void Apply()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        float z = (anchor != null ? anchor.position.z : transform.position.z);
        sr.sortingOrder = Mathf.RoundToInt(-z * 10f) + 100 + offset;
    }
}
