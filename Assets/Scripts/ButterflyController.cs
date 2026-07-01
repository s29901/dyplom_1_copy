using UnityEngine;

public class ButterflyController : MonoBehaviour
{
    private Vector3 targetPos;
    private float speed;
    private SpriteRenderer sr;
    private float minX, maxX, minY, maxY;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        speed = Random.Range(1.5f, 3.5f);
        
        // liczymy granice jeden raz na starcie
        Camera cam = Camera.main;
        float height = cam.orthographicSize;
        float width = height * cam.aspect;
        float margin = 0.5f;
        
        minX = cam.transform.position.x - width + margin;
        maxX = cam.transform.position.x + width - margin;
        minY = cam.transform.position.y - height + margin;
        maxY = cam.transform.position.y + height - margin;
        
        SetNewTarget();
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        // wymuszamy ograniczenie pozycji do granic
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, minX, maxX),
            Mathf.Clamp(transform.position.y, minY, maxY),
            transform.position.z
        );

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
            Random.Range(minY, maxY),
            transform.position.z
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