using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Повесить на Hero в каждой сцене.
// Запоминает, где герой стоял в каждой сцене (позицию сохраняет портал
// в момент входа), и при возвращении в сцену ставит героя на то же место.
// Работает в рамках одной игровой сессии.
public class HeroSpawn : MonoBehaviour
{
    [Header("Отойти от портала при возвращении")]
    [Tooltip("На сколько единиц сместить героя от места входа, чтобы он " +
             "не появлялся внутри портала и сразу не улетал обратно")]
    public float stepAwayFromPortal = 1.5f;

    private static readonly Dictionary<string, Vector3> savedPositions =
        new Dictionary<string, Vector3>();

    // Вызывается порталом при входе героя
    public static void SavePosition(Transform hero)
    {
        savedPositions[SceneManager.GetActiveScene().name] = hero.position;
    }

    // Забыть все запомненные места: новая игра начинается с точек по умолчанию
    public static void ClearSaved()
    {
        savedPositions.Clear();
    }

    private void Start()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (!savedPositions.TryGetValue(scene, out Vector3 pos)) return;

        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        transform.position = pos;
        Physics.SyncTransforms();
        Unstuck(cc);

        if (cc != null) cc.enabled = true;
    }

    // Если герой оказался внутри стены или коллайдера портала —
    // мягко выталкиваем его наружу, иначе он не сможет сдвинуться с места
    private void Unstuck(CharacterController cc)
    {
        float radius = cc != null ? cc.radius * MaxScale() : 0.5f;
        Vector3 center = transform.position + Vector3.up * radius;

        var hits = Physics.OverlapSphere(center, radius);
        Vector3 push = Vector3.zero;
        int blockers = 0;

        foreach (var col in hits)
        {
            if (col == null || col.isTrigger) continue;
            if (col.transform.IsChildOf(transform)) continue;      // собственные коллайдеры
            if (col.gameObject.layer == LayerMask.NameToLayer("Ground")) continue;

            // направление «прочь» от препятствия
            Vector3 closest = col.ClosestPoint(center);
            Vector3 away = center - closest;
            if (away.sqrMagnitude < 0.0001f) away = -transform.forward;
            away.y = 0f;
            push += away.normalized;
            blockers++;
        }

        if (blockers == 0) return;

        Vector3 offset = push.normalized * Mathf.Max(stepAwayFromPortal, radius * 2f);
        transform.position += new Vector3(offset.x, 0f, offset.z);
        Physics.SyncTransforms();

        Debug.Log($"[HeroSpawn] Точка возврата была занята ({blockers} коллайдер(ов)) — герой отодвинут");
    }

    private float MaxScale()
    {
        var s = transform.lossyScale;
        return Mathf.Max(s.x, Mathf.Max(s.y, s.z));
    }
}
