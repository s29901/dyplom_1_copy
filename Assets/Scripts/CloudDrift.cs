using UnityEngine;

// Фоновое облако, плывущее по небу.
// Повесить на объект со SpriteRenderer. Облако движется по X,
// слегка покачивается по вертикали и, уплыв за край, появляется
// с другой стороны — небо никогда не пустеет.
//
// Для параллакса: у дальних облаков ставь меньше speed и alpha,
// у ближних — больше. Проще всего расставить их через CloudSpawner.
public class CloudDrift : MonoBehaviour
{
    [Header("Движение")]
    public float speed = 0.5f;            // скорость по X (минус = влево)
    [Range(0f, 0.5f)]
    public float speedVariation = 0.3f;   // случайный разброс скорости ±30%

    [Header("Покачивание")]
    public float bobAmplitude = 0.15f;    // насколько облако качается вверх-вниз
    public float bobSpeed = 0.3f;         // как быстро качается

    [Header("Границы (мировые X). Совпадают = взять с камеры")]
    public float leftBound = 0f;
    public float rightBound = 0f;
    public float margin = 3f;             // запас за краем экрана

    [Header("Плавное появление у краёв")]
    public bool fadeAtEdges = false;
    public float fadeDistance = 4f;       // на каком расстоянии от края таять

    private SpriteRenderer sr;
    private float baseY;
    private float bobPhase;
    private float actualSpeed;
    private float baseAlpha;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        baseY = transform.position.y;
        bobPhase = Random.Range(0f, Mathf.PI * 2f);
        actualSpeed = speed * (1f + Random.Range(-speedVariation, speedVariation));
        baseAlpha = sr != null ? sr.color.a : 1f;

        if (Mathf.Approximately(leftBound, rightBound))
            AutoBounds();
    }

    // Если границы не заданы вручную — берём ширину обзора камеры
    private void AutoBounds()
    {
        var cam = Camera.main;
        if (cam == null) { leftBound = -20f; rightBound = 20f; return; }

        float halfWidth;
        if (cam.orthographic)
            halfWidth = cam.orthographicSize * cam.aspect;
        else
        {
            float dist = Mathf.Abs(cam.transform.position.z - transform.position.z);
            halfWidth = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * dist * cam.aspect;
        }

        // Запас должен быть шире самого облака, иначе перескок будет виден
        float cloudHalfWidth = sr != null ? sr.bounds.extents.x : 0f;
        float pad = Mathf.Max(margin, cloudHalfWidth + 1f);

        leftBound = cam.transform.position.x - halfWidth - pad;
        rightBound = cam.transform.position.x + halfWidth + pad;
    }

    void Update()
    {
        // Плывём
        Vector3 p = transform.position;
        p.x += actualSpeed * Time.deltaTime;

        // Мягко качаемся по вертикали
        p.y = baseY + Mathf.Sin(Time.time * bobSpeed + bobPhase) * bobAmplitude;
        transform.position = p;

        Wrap();
        FadeNearEdges();
    }

    // Уплыло за край — возвращаем с противоположной стороны
    private void Wrap()
    {
        Vector3 p = transform.position;
        if (actualSpeed > 0f && p.x > rightBound)
        {
            p.x = leftBound;
            transform.position = p;
            Reroll();
        }
        else if (actualSpeed < 0f && p.x < leftBound)
        {
            p.x = rightBound;
            transform.position = p;
            Reroll();
        }
    }

    // При новом заходе слегка меняем высоту и скорость,
    // чтобы цикл не читался как повтор
    private void Reroll()
    {
        baseY += Random.Range(-0.6f, 0.6f);
        actualSpeed = speed * (1f + Random.Range(-speedVariation, speedVariation));
    }

    private void FadeNearEdges()
    {
        if (!fadeAtEdges || sr == null) return;

        float x = transform.position.x;
        float distToEdge = Mathf.Min(x - leftBound, rightBound - x);
        float k = Mathf.Clamp01(distToEdge / Mathf.Max(fadeDistance, 0.01f));

        Color c = sr.color;
        c.a = baseAlpha * k;
        sr.color = c;
    }
}
