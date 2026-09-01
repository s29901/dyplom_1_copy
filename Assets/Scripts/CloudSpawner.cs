using UnityEngine;

// Наполняет небо облаками: сам расставляет их по слоям с параллаксом.
// Повесить на пустой объект в сцене, накидать спрайты облаков — и всё.
//
// Слои: 0 — самый дальний (мелкий, бледный, медленный),
// последний — самый ближний. Настраивается диапазонами ниже.
public class CloudSpawner : MonoBehaviour
{
    [Header("Спрайты облаков (берутся случайно)")]
    public Sprite[] cloudSprites;

    [Header("Слои параллакса")]
    public int layers = 3;
    public int cloudsPerLayer = 4;

    [Header("Дальний слой")]
    public float farScale = 0.6f;
    public float farSpeed = 0.15f;
    [Range(0f, 1f)] public float farAlpha = 0.45f;

    [Header("Ближний слой")]
    public float nearScale = 1.4f;
    public float nearSpeed = 0.6f;
    [Range(0f, 1f)] public float nearAlpha = 0.95f;

    [Header("Область неба (относительно этого объекта)")]
    public float width = 40f;         // ширина полосы, по которой плывут облака
    public float minY = 3f;           // нижняя граница неба
    public float maxY = 8f;           // верхняя граница
    public float depthZ = 10f;        // Z-позиция облаков (дальше героя)

    [Header("Прочее")]
    public bool moveRight = true;             // направление ветра
    public string sortingLayer = "Default";
    public int sortingOrderBase = -50;        // за героем; для облаков поверх — увеличь
    public float bobAmplitude = 0.15f;

    void Start()
    {
        if (cloudSprites == null || cloudSprites.Length == 0)
        {
            Debug.LogWarning("CloudSpawner: не заданы спрайты облаков");
            return;
        }

        float left = transform.position.x - width * 0.5f;
        float right = transform.position.x + width * 0.5f;

        for (int layer = 0; layer < layers; layer++)
        {
            // 0 = дальний, 1 = ближний
            float t = layers > 1 ? layer / (float)(layers - 1) : 1f;
            float scale = Mathf.Lerp(farScale, nearScale, t);
            float speed = Mathf.Lerp(farSpeed, nearSpeed, t) * (moveRight ? 1f : -1f);
            float alpha = Mathf.Lerp(farAlpha, nearAlpha, t);

            for (int i = 0; i < cloudsPerLayer; i++)
            {
                // равномерно раскидываем по ширине + случайный сдвиг
                float x = Mathf.Lerp(left, right, (i + Random.value) / cloudsPerLayer);
                float y = Random.Range(minY, maxY);
                CreateCloud(x, y, scale, speed, alpha, layer, left, right);
            }
        }
    }

    private void CreateCloud(float x, float y, float scale, float speed,
                             float alpha, int layer, float left, float right)
    {
        var go = new GameObject($"Cloud_L{layer}");
        go.transform.SetParent(transform);
        go.transform.position = new Vector3(x, y, depthZ - layer * 0.5f);
        go.transform.localScale = Vector3.one * scale * Random.Range(0.85f, 1.15f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = cloudSprites[Random.Range(0, cloudSprites.Length)];
        sr.color = new Color(1f, 1f, 1f, alpha);
        sr.sortingLayerName = sortingLayer;
        sr.sortingOrder = sortingOrderBase + layer;
        if (Random.value < 0.5f) sr.flipX = true; // чтобы формы не повторялись

        var drift = go.AddComponent<CloudDrift>();
        drift.speed = speed;
        drift.leftBound = left;
        drift.rightBound = right;
        drift.randomizeOnStart = true;               // своя траектория у каждого облака
        drift.amplitude = bobAmplitude * Mathf.Max(scale, 0.1f);
        drift.frequency = Random.Range(0.2f, 0.45f);
        drift.fadeAtEdges = false;
    }

    // Показывает полосу неба в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.6f);
        Vector3 c = new Vector3(transform.position.x, (minY + maxY) * 0.5f, depthZ);
        Gizmos.DrawWireCube(c, new Vector3(width, maxY - minY, 0.1f));
    }
}
