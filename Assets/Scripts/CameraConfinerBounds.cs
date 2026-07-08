using UnityEngine;

// Строит BoxCollider — зону, в которой разрешено находиться камере,
// по границам спрайта фона (та же математика, что была в CameraFollow).
// Использование: пустой объект в (0,0,0) + BoxCollider (Is Trigger) + этот скрипт.
// Назначь background и cam, затем правый клик по компоненту → Rebuild Bounds.
// Этот BoxCollider подаётся в CinemachineConfiner3D → Bounding Volume.
[RequireComponent(typeof(BoxCollider))]
public class CameraConfinerBounds : MonoBehaviour
{
    public SpriteRenderer background;  // спрайт фона (границы видимости)
    public Camera cam;                 // Main Camera (наклон, ortho size, aspect)
    public float extraMinZ = 30f;      // тот же запас, что был в CameraFollow
    public float boxHeight = 20f;      // толщина зоны по Y (просто с запасом вокруг камеры)

    private void Start()
    {
        Rebuild();
    }

    [ContextMenu("Rebuild Bounds")]
    public void Rebuild()
    {
        if (background == null || cam == null)
        {
            Debug.LogWarning("CameraConfinerBounds: назначь background и cam");
            return;
        }

        Bounds b = background.bounds;
        Vector3 fwd = cam.transform.forward;

        float orthH = cam.orthographicSize;
        float halfW = orthH * cam.aspect;
        float minX = b.min.x + halfW + orthH * 0.5f;
        float maxX = b.max.x - halfW - orthH * 0.5f;

        float bgZ = background.transform.position.z;
        float camY = cam.transform.position.y;
        float cosA = fwd.z;
        float tanA = -fwd.y / fwd.z;
        float viewHalfH = orthH / cosA;

        float camZTop = bgZ + (b.max.y - camY - viewHalfH) / tanA;
        float camZBottom = bgZ + (b.min.y - camY + viewHalfH) / tanA;
        float minZ = Mathf.Min(camZTop, camZBottom) - extraMinZ;
        float maxZ = Mathf.Max(camZTop, camZBottom);

        var box = GetComponent<BoxCollider>();
        box.isTrigger = true;

        // Переводим мировые границы в локальные координаты этого объекта
        Vector3 worldCenter = new Vector3((minX + maxX) * 0.5f, camY, (minZ + maxZ) * 0.5f);
        box.center = transform.InverseTransformPoint(worldCenter);
        box.size = new Vector3(Mathf.Max(0.01f, maxX - minX), boxHeight, Mathf.Max(0.01f, maxZ - minZ));

        Debug.Log($"CameraConfinerBounds → X: {minX:F2}..{maxX:F2} | Z: {minZ:F2}..{maxZ:F2}");
    }
}
