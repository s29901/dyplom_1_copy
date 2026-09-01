using UnityEngine;

// Фоновое облако, плывущее по небу.
// Повесить на объект со SpriteRenderer. Облако движется по X по одной
// из траекторий и, уплыв за край, появляется с другой стороны.
//
// Для параллакса: у дальних облаков меньше speed и alpha, у ближних больше.
public class CloudDrift : MonoBehaviour
{
    public enum Trajectory
    {
        Straight,   // ровно по прямой
        Wave,       // плавная синусоида вверх-вниз
        Diagonal,   // медленно поднимается или опускается
        Wander      // свободное блуждание (шум Перлина) — самое живое
    }

    [Header("Траектория")]
    public Trajectory trajectory = Trajectory.Wave;

    [Tooltip("Выбрать траекторию и параметры случайно при старте — " +
             "тогда одинаково настроенные облака полетят по-разному")]
    public bool randomizeOnStart = true;

    [Header("Движение")]
    public float speed = 0.5f;            // скорость по X (минус = влево)
    [Range(0f, 0.5f)]
    public float speedVariation = 0.3f;   // случайный разброс скорости ±30%

    [Header("Форма траектории")]
    public float amplitude = 0.4f;        // размах по вертикали (Wave / Wander)
    public float frequency = 0.25f;       // как быстро колеблется
    public float diagonalSlope = 0.08f;   // подъём/спуск для Diagonal

    [Header("Границы (мировые X). Совпадают = взять с камеры")]
    public float leftBound = 0f;
    public float rightBound = 0f;
    public float margin = 3f;

    [Header("Плавное появление у краёв")]
    public bool fadeAtEdges = false;
    public float fadeDistance = 4f;

    private SpriteRenderer sr;
    private float baseY;
    private float phase;
    private float noiseSeed;
    private float actualSpeed;
    private float baseAlpha;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        baseY = transform.position.y;
        baseAlpha = sr != null ? sr.color.a : 1f;

        if (randomizeOnStart) Randomize();
        else
        {
            phase = Random.Range(0f, Mathf.PI * 2f);
            noiseSeed = Random.Range(0f, 100f);
            actualSpeed = speed * (1f + Random.Range(-speedVariation, speedVariation));
        }

        if (Mathf.Approximately(leftBound, rightBound))
            AutoBounds();
    }

    // Своя траектория и свой характер движения для каждого облака
    private void Randomize()
    {
        trajectory = (Trajectory)Random.Range(0, 4);
        phase = Random.Range(0f, Mathf.PI * 2f);
        noiseSeed = Random.Range(0f, 100f);
        actualSpeed = speed * (1f + Random.Range(-speedVariation, speedVariation));
        amplitude *= Random.Range(0.5f, 1.6f);
        frequency *= Random.Range(0.6f, 1.5f);
        diagonalSlope *= Random.Range(-1f, 1f); // одни поднимаются, другие опускаются
    }

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

        float cloudHalfWidth = sr != null ? sr.bounds.extents.x : 0f;
        float pad = Mathf.Max(margin, cloudHalfWidth + 1f);

        leftBound = cam.transform.position.x - halfWidth - pad;
        rightBound = cam.transform.position.x + halfWidth + pad;
    }

    void Update()
    {
        Vector3 p = transform.position;
        p.x += actualSpeed * Time.deltaTime;
        p.y = VerticalOffset(p.x);
        transform.position = p;

        Wrap();
        FadeNearEdges();
    }

    // Высота облака в зависимости от выбранной траектории
    private float VerticalOffset(float x)
    {
        switch (trajectory)
        {
            case Trajectory.Straight:
                return baseY;

            case Trajectory.Wave:
                return baseY + Mathf.Sin(Time.time * frequency + phase) * amplitude;

            case Trajectory.Diagonal:
                // равномерный подъём/спуск на пути от края до края
                float t = Mathf.InverseLerp(leftBound, rightBound, x);
                return baseY + (t - 0.5f) * diagonalSlope * 20f;

            case Trajectory.Wander:
                // шум Перлина: неровное, «настоящее» блуждание
                float n = Mathf.PerlinNoise(noiseSeed, Time.time * frequency * 0.5f);
                return baseY + (n - 0.5f) * 2f * amplitude;

            default:
                return baseY;
        }
    }

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

    // При новом заходе слегка меняем высоту, скорость и характер пути
    private void Reroll()
    {
        baseY += Random.Range(-0.6f, 0.6f);
        actualSpeed = speed * (1f + Random.Range(-speedVariation, speedVariation));
        noiseSeed = Random.Range(0f, 100f);
        phase = Random.Range(0f, Mathf.PI * 2f);
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
