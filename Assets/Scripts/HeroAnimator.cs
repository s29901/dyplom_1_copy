using UnityEngine;

// Анимация ходьбы героини: три набора кадров (лицом, спиной, в профиль).
// Направление определяется по фактическому движению, профиль отражается по X.
//
// Повесить на Hero (рядом с HeroMovement). Кадры можно не назначать:
// они подхватятся из Resources/hero, если положить их туда.
[RequireComponent(typeof(SpriteRenderer))]
public class HeroAnimator : MonoBehaviour
{
    [Header("Кадры ходьбы")]
    public Sprite[] front;   // идёт к камере
    public Sprite[] back;    // идёт от камеры
    public Sprite[] side;    // идёт вбок (кадры смотрят вправо)

    [Header("Кадры покоя (стоит на месте)")]
    [Tooltip("Несколько кадров — героиня будет дышать/переминаться. " +
             "Пусто — возьмётся первый кадр ходьбы в этом направлении.")]
    public Sprite[] idleFrontFrames;
    public Sprite[] idleBackFrames;
    public Sprite[] idleSideFrames;
    public float idleFrameRate = 4f;       // покой медленнее ходьбы

    [Header("Настройки")]
    public float frameRate = 10f;          // кадров в секунду
    public float moveThreshold = 0.02f;    // ниже этой скорости считаем, что стоит

    [Range(0f, 1f)]
    [Tooltip("Насколько одна ось должна перевешивать другую, чтобы сменить направление. " +
             "Больше значение — увереннее держится текущий набор кадров на диагонали.")]
    public float directionBias = 0.35f;
    [Tooltip("В какую сторону смотрит героиня на кадрах профиля. " +
             "Если в игре она идёт спиной вперёд — переключи эту галочку.")]
    public bool sideFacesRight = false;

    private SpriteRenderer sr;
    private Vector3 lastPos;
    private float timer;
    private int frame;

    private enum Dir { Front, Back, Side }
    private Dir dir = Dir.Front;
    private bool faceLeft;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        lastPos = transform.position;
        ApplyIdle();
    }

    void Update()
    {
        // Скорость по горизонтали (Y не участвует — герой ходит по плоскости)
        Vector3 delta = transform.position - lastPos;
        lastPos = transform.position;
        delta.y = 0f;

        float speed = delta.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);

        bool moving = speed >= moveThreshold;

        if (moving) UpdateDirection(delta);

        // Смена состояния — начинаем анимацию заново
        if (moving != wasMoving)
        {
            wasMoving = moving;
            timer = 0f;
            frame = 0;
        }

        Animate(moving);
    }

    private bool wasMoving;

    // Куда смотрит герой: вбок, к камере или от камеры.
    //
    // На диагонали |dx| и |dz| почти равны, поэтому направление меняется
    // только когда одна ось заметно перевешивает другую (directionBias).
    // Иначе набор кадров дёргался бы каждый кадр.
    private void UpdateDirection(Vector3 delta)
    {
        float x = Mathf.Abs(delta.x);
        float z = Mathf.Abs(delta.z);
        float margin = 1f + Mathf.Max(directionBias, 0f);

        if (dir == Dir.Side)
        {
            // уходим из профиля, только если глубина явно преобладает
            if (z > x * margin)
                dir = delta.z < 0f ? Dir.Front : Dir.Back;
        }
        else
        {
            // переходим в профиль, только если горизонталь явно преобладает
            if (x > z * margin)
                dir = Dir.Side;
            else
                dir = delta.z < 0f ? Dir.Front : Dir.Back;  // -Z = к камере
        }

        // сторону профиля обновляем всегда, пока есть горизонтальное движение
        if (Mathf.Abs(delta.x) > 0.0001f)
            faceLeft = delta.x < 0f;
    }

    private void Animate(bool moving)
    {
        var set = moving ? CurrentSet() : CurrentIdleSet();

        // Кадров покоя нет — просто стоим на первом кадре ходьбы
        if (set == null || set.Length == 0)
        {
            ApplyIdle();
            return;
        }

        timer += Time.deltaTime;
        float step = 1f / Mathf.Max(moving ? frameRate : idleFrameRate, 0.01f);
        if (timer >= step)
        {
            timer -= step;
            frame = (frame + 1) % set.Length;
        }

        sr.sprite = set[Mathf.Clamp(frame, 0, set.Length - 1)];
        ApplyFlip();
    }

    private Sprite[] CurrentIdleSet() => dir switch
    {
        Dir.Back => idleBackFrames,
        Dir.Side => idleSideFrames,
        _        => idleFrontFrames,
    };

    private void ApplyIdle()
    {
        if (sr == null) return;

        Sprite s = dir switch
        {
            Dir.Back => First(idleBackFrames) ?? First(back),
            Dir.Side => First(idleSideFrames) ?? First(side),
            _        => First(idleFrontFrames) ?? First(front),
        };

        if (s != null) sr.sprite = s;
        ApplyFlip();
    }

    private void ApplyFlip()
    {
        // Отражаем только профиль: кадры нарисованы в одну сторону
        sr.flipX = (dir == Dir.Side) && (faceLeft == sideFacesRight);
    }

    private Sprite[] CurrentSet() => dir switch
    {
        Dir.Back => back,
        Dir.Side => side,
        _        => front,
    };

    private static Sprite First(Sprite[] a) => (a != null && a.Length > 0) ? a[0] : null;
}
