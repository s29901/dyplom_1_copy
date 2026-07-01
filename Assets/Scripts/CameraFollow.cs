using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Межі фону")]
    [SerializeField] private SpriteRenderer background;
    [SerializeField] private float extra_min_z = 30f;

    private Vector3 offset;
    private float minX, maxX, minZ, maxZ;

    void Start()
    {
        offset = transform.position - target.position;

        if (background != null)
        {
            Camera cam = Camera.main;
            Vector3 fwd = cam.transform.forward;
            Bounds b = background.bounds;

            float orthH = cam.orthographicSize;
            float halfW = orthH * cam.aspect;
            minX = b.min.x + halfW + cam.orthographicSize * 0.5f;
            maxX = b.max.x - halfW - cam.orthographicSize * 0.5f;

            // Kamera jest przechylona: fwd.z = cos(kąta), fwd.y = -sin(kąta)
            // Aby znaleźć Z kamery, przy którym brzeg ekranu zgadza się z brzegiem tła po Y,
            // używamy projekcji poprzez kąt przechylenia
            float bgZ   = background.transform.position.z;
            float camY  = transform.position.y;
            float cosA  = fwd.z;
            float tanA  = -fwd.y / fwd.z;
            float viewHalfH = orthH / cosA; // pokrycie po Y tła od centrum do brzegu ekranu

            float camZTop    = bgZ + (b.max.y - camY - viewHalfH) / tanA;
            float camZBottom = bgZ + (b.min.y - camY + viewHalfH) / tanA;
            minZ = Mathf.Min(camZTop, camZBottom) - extra_min_z;
            maxZ = Mathf.Max(camZTop, camZBottom);

            Debug.Log($"CameraBounds → minX={minX:F2} maxX={maxX:F2} | minZ={minZ:F2} maxZ={maxZ:F2} | halfW={halfW:F2}");
        }
    }

    void LateUpdate()
    {
        Vector3 desired = target.position + offset;

        if (background != null)
        {
            desired.x = Mathf.Clamp(desired.x, minX, maxX);
            desired.z = Mathf.Clamp(desired.z, minZ, maxZ);
        }

        transform.position = desired;
    }
}