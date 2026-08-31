using UnityEngine;

// Лампа-указатель в хабе.
// Горит только та лампа, чей квест сейчас следующий, — она показывает,
// куда идти. Когда пройдены все квесты, загораются все лампы.
//
// Повесить на объект лампы со SpriteRenderer, задать Quest Number (1-4)
// и оба спрайта. Порядок квестов берётся из ProgressManager.
public class QuestLamp : MonoBehaviour
{
    [Header("К какому квесту относится лампа (1-4)")]
    public int questNumber = 1;

    [Header("Спрайты")]
    public Sprite offSprite;
    public Sprite onSprite;

    [Header("Свечение (необязательно)")]
    public GameObject glowObject;      // дочерний объект: ореол, частицы, свет

    [Header("Пульсация зажжённой лампы")]
    public bool pulse = true;
    public float pulseSpeed = 1.2f;
    [Range(0f, 0.5f)] public float pulseStrength = 0.12f;

    private SpriteRenderer sr;
    private bool isOn;
    private float baseAlpha;
    private float phase;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        phase = Random.Range(0f, Mathf.PI * 2f); // чтобы лампы не мигали синхронно
        Refresh();
    }

    // Пересчитать состояние (вызывается на старте сцены)
    public void Refresh()
    {
        isOn = ShouldBeOn();

        if (sr != null)
        {
            var s = isOn ? onSprite : offSprite;
            if (s != null) sr.sprite = s;
            baseAlpha = sr.color.a;
        }

        if (glowObject != null && glowObject.activeSelf != isOn)
            glowObject.SetActive(isOn);
    }

    private bool ShouldBeOn()
    {
        var pm = ProgressManager.Instance;
        if (pm == null) return questNumber == 1; // нет менеджера — светим на первый

        // Все квесты пройдены — горят все лампы
        if (pm.QuestsCompleted() >= 4) return true;

        return questNumber == NextQuest(pm);
    }

    // Первый непройденный квест
    public static int NextQuest(ProgressManager pm)
    {
        if (!pm.quest1Done) return 1;
        if (!pm.quest2Done) return 2;
        if (!pm.quest3Done) return 3;
        if (!pm.quest4Done) return 4;
        return 0;
    }

    void Update()
    {
        if (!pulse || !isOn || sr == null) return;

        // Мягкое дыхание света
        float k = 1f + Mathf.Sin(Time.time * pulseSpeed + phase) * pulseStrength;
        var c = sr.color;
        c.a = Mathf.Clamp01(baseAlpha * k);
        sr.color = c;
    }
}
