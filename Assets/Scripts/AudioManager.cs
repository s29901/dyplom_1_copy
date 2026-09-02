using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Музыка и звуки игры.
//
// Ничего настраивать не нужно: объект создаётся сам при запуске,
// живёт между сценами и берёт файлы из Assets/Resources/audio.
//
// Музыка по сценам:
//   меню, интро, хаб (пока не пройдены все квесты) — start_game
//   хаб после всех квестов и финал                — final_scene
//   Q1, Q2, Q3                                    — q1_q2_q3
//   Q4                                            — q4
//
// Звуки: AudioManager.PlaySfx("tree_transform") и т.п.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Громкость")]
    [Range(0f, 1f)] public float musicVolume = 0.6f;
    [Range(0f, 1f)] public float sfxVolume = 0.9f;

    [Header("Переход между треками")]
    public float crossfadeTime = 1.5f;

    [Header("Клик мышью")]
    public bool globalClickSound = true;
    public string clickSfx = "mouse";

    private AudioSource musicA, musicB, sfx;
    private AudioSource activeMusic;
    private string currentTrack = "";
    private readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (Instance != null) return;
        var go = new GameObject("~AudioManager");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<AudioManager>();
        Instance.Build();
    }

    private void Build()
    {
        musicA = gameObject.AddComponent<AudioSource>();
        musicB = gameObject.AddComponent<AudioSource>();
        sfx = gameObject.AddComponent<AudioSource>();

        foreach (var s in new[] { musicA, musicB })
        {
            s.loop = true;
            s.playOnAwake = false;
            s.volume = 0f;
            s.spatialBlend = 0f; // музыка не привязана к месту
        }
        sfx.playOnAwake = false;
        sfx.spatialBlend = 0f;

        activeMusic = musicA;

        musicVolume = PlayerPrefs.GetFloat("vol_music", musicVolume);
        sfxVolume = PlayerPrefs.GetFloat("vol_sfx", sfxVolume);

        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    // ---------- Музыка ----------

    public void PlayMusicForScene(string sceneName)
    {
        string track = TrackForScene(sceneName);
        PlayMusic(track);
    }

    private static string TrackForScene(string scene)
    {
        bool allQuestsDone = ProgressManager.Instance != null &&
                             ProgressManager.Instance.QuestsCompleted() >= 4;

        switch (scene)
        {
            case "Q1_Warmth_Mountain":
            case "Q2_Tears_Valley":
            case "Q3_Voice_Forest":
                return "q1_q2_q3";

            case "Q4_Play_Field":
                return "q4";

            case "99_Ending":
                return "final_scene";

            case "02_HubGarden":
                return allQuestsDone ? "final_scene" : "start_game";

            default: // меню, интро и всё остальное
                return "start_game";
        }
    }

    public void PlayMusic(string clipName)
    {
        if (string.IsNullOrEmpty(clipName) || clipName == currentTrack) return;

        var clip = Load(clipName);
        if (clip == null) return;

        currentTrack = clipName;

        var next = (activeMusic == musicA) ? musicB : musicA;
        next.clip = clip;
        next.Play();

        StopAllCoroutines();
        StartCoroutine(Crossfade(activeMusic, next));
        activeMusic = next;
    }

    private IEnumerator Crossfade(AudioSource from, AudioSource to)
    {
        float t = 0f;
        float startVol = from != null ? from.volume : 0f;

        while (t < crossfadeTime)
        {
            t += Time.unscaledDeltaTime;
            float k = t / crossfadeTime;
            if (from != null) from.volume = Mathf.Lerp(startVol, 0f, k);
            to.volume = Mathf.Lerp(0f, musicVolume, k);
            yield return null;
        }

        if (from != null) { from.volume = 0f; from.Stop(); }
        to.volume = musicVolume;
    }

    // ---------- Звуки ----------

    public static void PlaySfx(string clipName, float volumeScale = 1f)
    {
        if (Instance == null || string.IsNullOrEmpty(clipName)) return;

        var clip = Instance.Load(clipName);
        if (clip == null) return;

        Instance.sfx.PlayOneShot(clip, Instance.sfxVolume * volumeScale);
    }

    // Звук в конкретной точке сцены (например, у зверька)
    public static void PlaySfxAt(string clipName, Vector3 position, float volumeScale = 1f)
    {
        if (Instance == null) return;
        var clip = Instance.Load(clipName);
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, Instance.sfxVolume * volumeScale);
    }

    private AudioClip Load(string name)
    {
        if (cache.TryGetValue(name, out var c)) return c;

        c = Resources.Load<AudioClip>("audio/" + name);
        if (c == null)
            Debug.LogWarning($"AudioManager: не найден Resources/audio/{name}");
        cache[name] = c;
        return c;
    }

    // ---------- Громкость ----------

    public void SetMusicVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        if (activeMusic != null) activeMusic.volume = musicVolume;
        PlayerPrefs.SetFloat("vol_music", musicVolume);
    }

    public void SetSfxVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat("vol_sfx", sfxVolume);
    }

    private bool pendingClick;
    private bool clickSuppressed;

    // Вызывать из объекта, у которого свой звук клика (например бабочки),
    // чтобы общий щелчок мыши в этот раз не звучал
    public static void SuppressClick()
    {
        if (Instance != null) Instance.clickSuppressed = true;
    }

    private void Update()
    {
        if (globalClickSound && Input.GetMouseButtonDown(0))
            pendingClick = true;
    }

    private void LateUpdate()
    {
        // Звук клика играем в конце кадра: к этому моменту объекты
        // со своим звуком уже успели попросить тишины
        if (pendingClick && !clickSuppressed)
            PlaySfx(clickSfx);

        pendingClick = false;
        clickSuppressed = false;
    }
}
