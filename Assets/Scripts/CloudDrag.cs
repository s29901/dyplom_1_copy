using UnityEngine;
using System.Collections;

public class CloudDrag : MonoBehaviour
{
    public enum PatrolPath
    {
        Circle,    // ровный круг вокруг исходной точки
        Figure8,   // восьмёрка
        Wander,    // свободное блуждание (шум Перлина)
        LineX      // покачивание только влево-вправо
    }

    [Header("Траектория патруля")]
    [SerializeField] private PatrolPath patrolPath = PatrolPath.Circle;
    [Tooltip("Выбрать траекторию и параметры случайно — облака полетят по-разному")]
    [SerializeField] private bool randomizePatrol = true;

    [SerializeField] private float patrolSpeed = 0.35f; // скорость обхода круга (меньше = спокойнее)
    [SerializeField] private float patrolRange = 0.6f;  // радиус круга
    [SerializeField] private float rainDuration = 3f;   // sekund przebywania nad drzewem
    [SerializeField] private Transform tree;             // дерево (запасной вариант с радиусом)
    [SerializeField] private float treeRadius = 2f;     // радиус, если зона не задана
    [SerializeField] private WarmthZone rainZone;        // настраиваемый бокс: дождь работает внутри него
    [SerializeField] private GameObject rainObject;      // obiekt potomny z animacją deszczu

    private Vector3 startPosition;
    private bool isDragging = false;
    private bool isReturning = false;
    private bool isDone = false;
    private float rainProgress = 0f;
    private float patrolAngle = 0f;
    private float noiseSeed;
    private Plane dragPlane;
    private SpriteRenderer spriteRenderer;

    // Informujemy QuestManager, kiedy chmura skończy padać
    public System.Action OnCloudDone;

    [Header("Отладка дождя (писать в Console)")]
    [SerializeField] private bool debugRain = true;

    // Печатает всё, что влияет на видимость дождя
    private void LogRainState()
    {
        var sr = rainObject.GetComponentInChildren<SpriteRenderer>(true);
        var anim = rainObject.GetComponentInChildren<Animator>(true);

        string s = $"[Дождь] облако '{name}' -> объект '{rainObject.name}' включён.\n";
        s += $"   позиция: {rainObject.transform.position}, масштаб: {rainObject.transform.lossyScale}\n";

        if (sr == null) s += "   ❌ SpriteRenderer не найден\n";
        else s += $"   рендерер: включён={sr.enabled}, спрайт={(sr.sprite ? sr.sprite.name : "НЕТ")}, " +
                  $"альфа={sr.color.a:0.00}, слой='{sr.sortingLayerName}', порядок={sr.sortingOrder}, виден камерой={sr.isVisible}\n";

        if (anim == null) s += "   ❌ Animator не найден";
        else s += $"   аниматор: включён={anim.enabled}, контроллер={(anim.runtimeAnimatorController ? anim.runtimeAnimatorController.name : "НЕТ")}, скорость={anim.speed}";

        Debug.Log(s, rainObject);
    }

    void Start()
    {
        // Квест уже пройден — облака своё отработали и больше не появляются
        if (ProgressManager.Instance != null && ProgressManager.Instance.quest2Done)
        {
            gameObject.SetActive(false);
            return;
        }

        startPosition = transform.position;
        dragPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));
        if (rainObject != null) rainObject.SetActive(false);

        // Своя траектория и свой ритм у каждого облака
        if (randomizePatrol)
        {
            patrolSpeed *= Random.Range(0.7f, 1.3f);   // у каждого свой темп
            patrolRange *= Random.Range(0.75f, 1.25f); // и свой радиус
            if (Random.value < 0.5f) patrolSpeed = -patrolSpeed; // кто-то по часовой, кто-то против
        }
        patrolAngle = Random.Range(0f, Mathf.PI * 2f); // разная начальная фаза
        noiseSeed = Random.Range(0f, 100f);

        // Chmury są rysowane nad bohaterem (są wyżej w scenie)
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) spriteRenderer.sortingOrder = 200;
    }

    void Update()
    {
        if (isDone) return;

        if (isDragging)
            HandleDrag();
        else if (!isReturning)
            HandlePatrol();
    }

    void HandlePatrol()
    {
        patrolAngle += Time.deltaTime * patrolSpeed;

        Vector3 offset;
        switch (patrolPath)
        {
            case PatrolPath.Figure8:
                // восьмёрка: по Z колебание вдвое быстрее, чем по X
                offset = new Vector3(
                    Mathf.Sin(patrolAngle) * patrolRange,
                    0,
                    Mathf.Sin(patrolAngle * 2f) * patrolRange * 0.4f);
                break;

            case PatrolPath.Wander:
                // шум Перлина: неровное, «живое» блуждание
                float nx = Mathf.PerlinNoise(noiseSeed, Time.time * 0.15f * Mathf.Abs(patrolSpeed));
                float nz = Mathf.PerlinNoise(noiseSeed + 50f, Time.time * 0.15f * Mathf.Abs(patrolSpeed));
                offset = new Vector3(
                    (nx - 0.5f) * 2f * patrolRange,
                    0,
                    (nz - 0.5f) * 2f * patrolRange * 0.5f);
                break;

            case PatrolPath.LineX:
                // только влево-вправо, без ухода в глубину
                offset = new Vector3(Mathf.Sin(patrolAngle) * patrolRange, 0, 0);
                break;

            default: // Circle — ровный круг вокруг точки расстановки
                offset = new Vector3(
                    Mathf.Sin(patrolAngle) * patrolRange,
                    0,
                    Mathf.Cos(patrolAngle) * patrolRange);
                break;
        }

        transform.position = startPosition + offset;
    }

    void HandleDrag()
    {
        // Przesuwamy chmurę za myszką
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float dist;
        if (dragPlane.Raycast(ray, out dist))
            transform.position = ray.GetPoint(dist);

        // Облако в зоне дождя?
        bool inRainZone;
        if (rainZone != null)
        {
            // Основной вариант: настраиваемый бокс
            inRainZone = rainZone.Contains(transform.position);
        }
        else if (tree != null)
        {
            // Запасной вариант: радиус вокруг дерева
            float distToTree = Vector2.Distance(
                new Vector2(transform.position.x, transform.position.z),
                new Vector2(tree.position.x, tree.position.z)
            );
            inRainZone = distToTree < treeRadius;
        }
        else
        {
            inRainZone = false;
        }

        if (inRainZone)
        {
            // Deszcz pada
            if (rainObject != null)
            {
                bool wasOff = !rainObject.activeSelf;
                rainObject.SetActive(true);
                if (wasOff && debugRain) LogRainState();
            }
            else if (debugRain)
            {
                Debug.LogWarning($"[Дождь] У облака '{name}' не назначен rainObject!");
            }
            rainProgress += Time.deltaTime;
            if (rainProgress >= rainDuration)
            {
                CompleteRain();
                return;
            }
        }
        else
        {
            // Nie nad drzewem, deszcz stop
            if (rainObject != null) rainObject.SetActive(false);
        }

        // Gracz zwolnił przycisk myszy
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            rainProgress = 0f;
            if (rainObject != null) rainObject.SetActive(false);
            StartCoroutine(ReturnToStart());
        }
    }

    void OnMouseDown()
    {
        if (!isDone && !isReturning)
            isDragging = true;
    }

    void CompleteRain()
    {
        isDone = true;
        isDragging = false;
        if (rainObject != null) rainObject.SetActive(false);
        OnCloudDone?.Invoke(); // informujemy QuestManager
        StartCoroutine(DisappearEffect());
    }

    IEnumerator DisappearEffect()
    {
        // Chmura płynnie zmniejsza się i zanika
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / 0.5f);
            yield return null;
        }
        gameObject.SetActive(false);
    }

    // Рисует зону дождя в Scene, когда облако выделено
    // (если задан бокс rainZone, он и так всегда подсвечен жёлтым)
    private void OnDrawGizmosSelected()
    {
        if (rainZone != null || tree == null) return;
        Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.9f);
        Gizmos.DrawWireSphere(tree.position, treeRadius);
    }

    IEnumerator ReturnToStart()
    {
        isReturning = true;
        float elapsed = 0f;
        Vector3 currentPos = transform.position;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(currentPos, startPosition, elapsed);
            yield return null;
        }
        patrolAngle = 0f; // aby nie było skoku
        isReturning = false;
    }
}