using UnityEngine;

public class SunDrag : MonoBehaviour
{
    // Płaszczyzna na wysokości Y=3, słoneczko lata tutaj
    private Plane sunPlane;

    void Start()
    {
        sunPlane = new Plane(Vector3.up, new Vector3(0, 3, 0));
    }

    void Update()
    {
        // Wysyłamy promień z kamery przez pozycję myszy
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance;

        // Znajdujemy, gdzie promień przecina płaszczyznę
        if (sunPlane.Raycast(ray, out distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            // Słoneczko podąża za myszką
            transform.position = worldPos;
        }
    }
}