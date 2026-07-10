using UnityEngine;

// Зона срабатывания WarmthBar. Настраивается прямо в редакторе:
// двигай объект и меняй размер BoxCollider (или scale объекта).
// Высота (Y) не учитывается — важно только положение по X/Z.
// Зона всегда подсвечена в Scene жёлтым.
[RequireComponent(typeof(BoxCollider))]
public class WarmthZone : MonoBehaviour
{
    private BoxCollider box;

    private void Awake()
    {
        box = GetComponent<BoxCollider>();
        box.isTrigger = true;
    }

    // Находится ли точка внутри зоны (по X/Z, высота игнорируется)
    public bool Contains(Vector3 worldPos)
    {
        if (box == null) box = GetComponent<BoxCollider>();
        Bounds b = box.bounds;
        return worldPos.x >= b.min.x && worldPos.x <= b.max.x &&
               worldPos.z >= b.min.z && worldPos.z <= b.max.z;
    }

    private void OnDrawGizmos()
    {
        var bc = GetComponent<BoxCollider>();
        if (bc == null) return;
        Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.25f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(bc.center, bc.size);
        Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.9f);
        Gizmos.DrawWireCube(bc.center, bc.size);
    }
}
