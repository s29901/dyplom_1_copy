using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Курсор игры: обычный (kursor) и «можно взаимодействовать» (kursor_eye).
//
// Ничего настраивать не нужно: объект создаётся сам при запуске игры,
// живёт между сценами и берёт картинки из Assets/Resources/cursors.
//
// Курсор меняется на kursor_eye, когда мышь над чем-то интерактивным:
// птица, зверьки, облака, солнце, пузыри, портал, кнопки интерфейса.
public class CursorController : MonoBehaviour
{
    [Header("Точка нажатия внутри картинки (0,0 = левый верхний угол)")]
    public Vector2 hotspot = new Vector2(4f, 2f);

    [Header("Дальность луча до объектов сцены")]
    public float rayDistance = 500f;

    private Texture2D normalCursor;
    private Texture2D activeCursor;
    private bool isActive;
    private bool applied; // был ли курсор установлен хоть раз

    // Скрипты, при наведении на которые курсор становится «глазом»
    private static readonly HashSet<string> interactiveScripts = new HashSet<string>
    {
        "BirdInteraction", "QuestBirdInteraction", "NPCInteraction",
        "EmotionAnimal", "CloudDrag", "SunDrag", "BubbleController",
        "ButterflyController", "FlowerPop"
        // PortalTrigger не включаем: портал срабатывает от подхода, а не от клика
    };

    // Создаётся автоматически при старте игры — добавлять в сцены не нужно
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (FindFirstObjectByType<CursorController>() != null) return;
        var go = new GameObject("~CursorController");
        DontDestroyOnLoad(go);
        go.AddComponent<CursorController>();
    }

    void Start()
    {
        normalCursor = Resources.Load<Texture2D>("cursors/kursor");
        activeCursor = Resources.Load<Texture2D>("cursors/kursor_eye");

        if (normalCursor == null)
            Debug.LogWarning("CursorController: не найден Resources/cursors/kursor");

        SetCursor(false);
    }

    void Update()
    {
        SetCursor(IsOverInteractive());
    }

    private bool IsOverInteractive()
    {
        // Над интерфейсом курсор обычный (на кнопках глаз не нужен)
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return false;

        var cam = Camera.main;
        if (cam == null) return false;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // Объекты с обычными (3D) коллайдерами
        foreach (var hit in Physics.RaycastAll(ray, rayDistance))
        {
            if (hit.collider == null || !hit.collider.enabled) continue;
            if (HasInteractiveScript(hit.collider.gameObject)) return true;
        }

        // Объекты с 2D-коллайдерами (например птица с PolygonCollider2D)
        foreach (var hit2d in Physics2D.GetRayIntersectionAll(ray, rayDistance))
        {
            if (hit2d.collider == null || !hit2d.collider.enabled) continue;
            if (HasInteractiveScript(hit2d.collider.gameObject)) return true;
        }

        return false;
    }

    private static bool HasInteractiveScript(GameObject go)
    {
        foreach (var mb in go.GetComponentsInParent<MonoBehaviour>())
        {
            if (mb == null || !mb.enabled) continue;
            if (interactiveScripts.Contains(mb.GetType().Name)) return true;
        }
        return false;
    }

    private void SetCursor(bool active)
    {
        if (applied && active == isActive) return; // состояние не изменилось
        isActive = active;
        applied = true;

        var tex = active ? activeCursor : normalCursor;
        if (tex == null) tex = normalCursor;      // «глаза» нет — оставим обычный
        if (tex != null)
        {
            Cursor.visible = true;
            Cursor.SetCursor(tex, hotspot, CursorMode.Auto);
        }
    }
}
