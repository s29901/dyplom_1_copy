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

    [Header("Зажигать только после разговора с птицей")]
    [Tooltip("Пока вступительный разговор не состоялся, все лампы погашены — " +
             "герой может гулять, но подсказки куда идти ещё нет")]
    public bool requireBirdTalk = true;

    [Tooltip("Тот же ключ, что в Dialogue Id у BirdInteraction")]
    public string birdDialogueId = "hub_bird_intro";

    [Header("Свечение (необязательно)")]
    public GameObject glowObject;      // дочерний объект: ореол, частицы, свет

    [Header("Звук зажжённой лампы (из Resources/audio)")]
    public bool lampSound = true;
    public string lampSfx = "light";
    public float soundVolume = 0.5f;
    public float soundRange = 12f;   // с какого расстояния слышно

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
        if (sr != null) baseAlpha = sr.color.a;  // до того, как пульсация начнёт менять alpha
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

            // погашенная лампа не пульсирует — возвращаем исходную прозрачность
            if (!isOn)
            {
                var c = sr.color;
                c.a = baseAlpha;
                sr.color = c;
            }
        }

        if (glowObject != null && glowObject.activeSelf != isOn)
            glowObject.SetActive(isOn);

        UpdateLampSound();
    }

    private AudioSource lampSource;

    // Зациклённый звук фонаря: слышен, только пока лампа горит,
    // и тем громче, чем ближе к ней герой
    private void UpdateLampSound()
    {
        if (!lampSound) { if (lampSource != null) lampSource.Stop(); return; }

        if (lampSource == null)
        {
            var clip = Resources.Load<AudioClip>("audio/" + lampSfx);
            if (clip == null) return;

            lampSource = gameObject.AddComponent<AudioSource>();
            lampSource.clip = clip;
            lampSource.loop = true;
            lampSource.playOnAwake = false;
            lampSource.spatialBlend = 1f;                     // звук из точки в пространстве
            lampSource.rolloffMode = AudioRolloffMode.Linear;
            lampSource.minDistance = 1.5f;
            lampSource.maxDistance = soundRange;
            lampSource.time = Random.Range(0f, clip.length);  // лампы не в унисон
        }

        float v = soundVolume * (AudioManager.Instance != null ? AudioManager.Instance.sfxVolume : 1f);
        lampSource.volume = v;

        if (isOn && !lampSource.isPlaying) lampSource.Play();
        else if (!isOn && lampSource.isPlaying) lampSource.Stop();
    }

    // Разговор помечается пройденным в момент запуска диалога,
    // поэтому лампа ждёт, пока панель закроется, и только тогда загорается.
    private float birdCheckTimer;

    private void WaitForBirdTalk()
    {
        if (!requireBirdTalk || isOn) return;

        birdCheckTimer -= Time.deltaTime;
        if (birdCheckTimer > 0f) return;
        birdCheckTimer = 0.25f;

        if (!BirdInteraction.WasPlayed(birdDialogueId)) return;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive) return;

        Refresh();
    }

    private bool ShouldBeOn()
    {
        // До вступительного разговора с птицей подсказок нет
        if (requireBirdTalk && !BirdInteraction.WasPlayed(birdDialogueId))
            return false;

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
        WaitForBirdTalk();

        if (!pulse || !isOn || sr == null) return;

        // Мягкое дыхание света
        float k = 1f + Mathf.Sin(Time.time * pulseSpeed + phase) * pulseStrength;
        var c = sr.color;
        c.a = Mathf.Clamp01(baseAlpha * k);
        sr.color = c;
    }
}
