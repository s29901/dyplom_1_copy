using UnityEngine;

// Солнце нужно схватить мышью (нажать на него) и тянуть.
// При отпускании плавно возвращается в исходное положение.
// Требует Collider (например SphereCollider) на этом же объекте.
public class SunDrag : MonoBehaviour
{
    [SerializeField] private float planeHeight = 3f;   // высота, на которой летает солнце
    [SerializeField] private float returnSpeed = 8f;   // скорость возврата (больше = быстрее)

    private Plane sunPlane;
    private bool dragging;
    private bool returning;
    private Vector3 grabOffset;
    private Vector3 startPosition; // исходное положение, куда возвращаемся

    void Start()
    {
        sunPlane = new Plane(Vector3.up, new Vector3(0, planeHeight, 0));

        // Ставим солнце на высоту плоскости, иначе при наклонной камере
        // его видимое положение не совпадает с реальными X/Z
        Vector3 p = transform.position;
        p.y = planeHeight;
        transform.position = p;

        startPosition = transform.position;
    }

    void OnMouseDown()
    {
        if (TryGetMouseOnPlane(out Vector3 p))
        {
            dragging = true;
            returning = false;
            grabOffset = transform.position - p;
        }
    }

    void OnMouseUp()
    {
        dragging = false;
        returning = true;
    }

    void Update()
    {
        if (dragging)
        {
            if (TryGetMouseOnPlane(out Vector3 p))
            {
                transform.position = p + grabOffset;
            }
        }
        else if (returning)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, startPosition, returnSpeed * Time.deltaTime);

            if (transform.position == startPosition)
                returning = false;
        }
    }

    private bool TryGetMouseOnPlane(out Vector3 pos)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (sunPlane.Raycast(ray, out float distance))
        {
            pos = ray.GetPoint(distance);
            return true;
        }
        pos = Vector3.zero;
        return false;
    }
}
