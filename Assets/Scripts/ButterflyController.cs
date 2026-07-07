using UnityEngine;

public class ButterflyController : MonoBehaviour
{
    [Header("Зона полёта (мировые координаты)")]
    public float minX = -10f;
    public float maxX = 10f;
    public float minZ = -15f;
    public float maxZ = 5f;

    [Header("Высота полёта над землёй")]
    public float flyHeight = 3f;

    private Vector3 targetPos;
    private float speed;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        speed = Random.Range(1.5f, 3.5f);

        // Фиксируем Y (высота) — бабочка летает на одном уровне
        Vector3 pos = transform.position;
        pos.y = flyHeight;
        transform.position = pos;

        SetNewTarget();
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position, targetPos, speed * Time.deltaTime);

        // Ограничиваем по X и Z, Y всегда фиксирован
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, minX, maxX),
            flyHeight,
            Mathf.Clamp(transform.position.z, minZ, maxZ)
        );

        // Флип спрайта по направлению движения
        if (targetPos.x < transform.position.x)
            sr.flipX = true;
        else
            sr.flipX = false;

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
            SetNewTarget();
    }

    void SetNewTarget()
    {
        targetPos = new Vector3(
            Random.Range(minX, maxX),
            flyHeight,
            Random.Range(minZ, maxZ)
        );
    }

    void OnMouseDown()
    {
        sr.color = new Color(Random.value, Random.value, Random.value);
        StartCoroutine(Jump());
        SpawnTrail();
    }

    System.Collections.IEnumerator Jump()
    {
        Vector3 original = transform.localScale;
        transform.localScale = original * 1.5f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = original;
    }

    void SpawnTrail()
    {
        GameObject trail = new GameObject("Trail");
        trail.transform.position = transform.position;
        SpriteRenderer trailSr = trail.AddComponent<SpriteRenderer>();
        trailSr.sprite = sr.sprite;
        trailSr.color = new Color(Random.value, Random.value, Random.value, 0.6f);
        trailSr.sortingOrder = -1;
        Destroy(trail, 2f);
    }
}
