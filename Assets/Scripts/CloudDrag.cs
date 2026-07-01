using UnityEngine;
using System.Collections;

public class CloudDrag : MonoBehaviour
{
    [SerializeField] private float patrolSpeed = 1f;    // prędkość patrolu
    [SerializeField] private float patrolRange = 2f;    // szerokość kołysania
    [SerializeField] private float rainDuration = 3f;   // sekund przebywania nad drzewem
    [SerializeField] private Transform tree;             // drzewo
    [SerializeField] private float treeRadius = 2f;     // strefa nad drzewem
    [SerializeField] private GameObject rainObject;      // obiekt potomny z animacją deszczu

    private Vector3 startPosition;
    private bool isDragging = false;
    private bool isReturning = false;
    private bool isDone = false;
    private float rainProgress = 0f;
    private float patrolAngle = 0f;
    private Plane dragPlane;
    private SpriteRenderer spriteRenderer;

    // Informujemy QuestManager, kiedy chmura skończy padać
    public System.Action OnCloudDone;

    void Start()
    {
        startPosition = transform.position;
        dragPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));
        if (rainObject != null) rainObject.SetActive(false);

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
        // Płynne kołysanie po X i Z, obejmuje całą scenę
        patrolAngle += Time.deltaTime * patrolSpeed;
        transform.position = startPosition + new Vector3(
            Mathf.Sin(patrolAngle) * patrolRange,
            0,
            Mathf.Cos(patrolAngle * 0.7f) * patrolRange * 0.5f
        );
    }

    void HandleDrag()
    {
        // Przesuwamy chmurę za myszką
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float dist;
        if (dragPlane.Raycast(ray, out dist))
            transform.position = ray.GetPoint(dist);

        // Nad drzewem?
        float distToTree = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.z),
            new Vector2(tree.position.x, tree.position.z)
        );

        if (distToTree < treeRadius)
        {
            // Deszcz pada
            if (rainObject != null) rainObject.SetActive(true);
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