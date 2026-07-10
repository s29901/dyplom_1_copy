using UnityEngine;

// Строит BoxCollider — зону, в которой разрешено находиться камере,
// чтобы в кадр не попадало ничего за пределами спрайта фона (fon).
// Работает с наклонной камерой (например 45°) и наклонным фоном.
//
// Использование: пустой объект в (0,0,0) + BoxCollider (Is Trigger) + этот скрипт.
// Назначь background (fon) и cameraTransform (CinemachineCamera),
// укажи orthoSize (= Lens -> Orthographic Size), затем
// правый клик по компоненту → Rebuild Bounds.
// Полученный BoxCollider подаётся в CinemachineConfiner3D → Bounding Volume.
[RequireComponent(typeof(BoxCollider))]
public class CameraConfinerBounds : MonoBehaviour
{
    public SpriteRenderer background;    // спрайт фона (fon)
    public SpriteRenderer[] extraBackgrounds; // дополнительные куски фона, если фон составной
    public Transform cameraTransform;    // CinemachineCamera (наклон и высота берутся отсюда)
    public float orthoSize = 5f;         // Lens -> Orthographic Size камеры
    public float aspect = 16f / 9f;      // соотношение сторон экрана
    public float boxHeight = 20f;        // толщина зоны по Y (с запасом вокруг камеры)

    private void Start()
    {
        Rebuild();
    }

    [ContextMenu("Rebuild Bounds")]
    public void Rebuild()
    {
        if (background == null || cameraTransform == null)
        {
            Debug.LogWarning("CameraConfinerBounds: назначь background и cameraTransform");
            return;
        }

        Vector3 right = cameraTransform.right; // горизонталь экрана
        Vector3 up = cameraTransform.up;       // вертикаль экрана

        // Общие границы: основной фон + дополнительные куски
        Bounds b = background.bounds;
        if (extraBackgrounds != null)
        {
            foreach (var sr in extraBackgrounds)
            {
                if (sr != null) b.Encapsulate(sr.bounds);
            }
        }

        // Проекция всех 8 углов bounds фона на оси экрана камеры
        float uMin = float.MaxValue, uMax = float.MinValue;
        float vMin = float.MaxValue, vMax = float.MinValue;
        for (int i = 0; i < 8; i++)
        {
            Vector3 c = new Vector3(
                (i & 1) == 0 ? b.min.x : b.max.x,
                (i & 2) == 0 ? b.min.y : b.max.y,
                (i & 4) == 0 ? b.min.z : b.max.z);
            float u = Vector3.Dot(c, right);
            float v = Vector3.Dot(c, up);
            uMin = Mathf.Min(uMin, u); uMax = Mathf.Max(uMax, u);
            vMin = Mathf.Min(vMin, v); vMax = Mathf.Max(vMax, v);
        }

        float halfW = orthoSize * aspect;
        float halfH = orthoSize;

        // Допустимый диапазон центра камеры в экранных осях
        float uCamMin = uMin + halfW, uCamMax = uMax - halfW;
        float vCamMin = vMin + halfH, vCamMax = vMax - halfH;

        if (uCamMin > uCamMax || vCamMin > vCamMax)
        {
            Debug.LogWarning("CameraConfinerBounds: фон меньше кадра — уменьши Ortho Size");
            return;
        }

        // Камера ездит по X/Z на фиксированной высоте Y.
        // Горизонталь экрана ~ мировой X, вертикаль пересчитываем в Z:
        // v = camY*up.y + camZ*up.z  =>  camZ = (v - camY*up.y) / up.z
        float camY = cameraTransform.position.y;
        float zA = (vCamMin - camY * up.y) / up.z;
        float zB = (vCamMax - camY * up.y) / up.z;
        float zMin = Mathf.Min(zA, zB), zMax = Mathf.Max(zA, zB);

        var box = GetComponent<BoxCollider>();
        box.isTrigger = true;
        Vector3 worldCenter = new Vector3((uCamMin + uCamMax) * 0.5f, camY, (zMin + zMax) * 0.5f);
        box.center = transform.InverseTransformPoint(worldCenter);
        box.size = new Vector3(uCamMax - uCamMin, boxHeight, Mathf.Max(0.01f, zMax - zMin));

        Debug.Log($"CameraConfinerBounds → X: {uCamMin:F2}..{uCamMax:F2} | Z: {zMin:F2}..{zMax:F2} | Y={camY:F2}");
    }
}
