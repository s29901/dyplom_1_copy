using UnityEngine;

// Глобальный прогресс игры. Живёт между сценами (DontDestroyOnLoad)
// и сохраняется на диск (PlayerPrefs) — переживает перезапуск игры.
// Сброс: Tools -> Reset Story Progress или кнопка New Game в меню.
public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance;

    // Какие квесты завершены (читать можно напрямую,
    // а выставлять — через SetQuestDone, чтобы сохранилось)
    public bool quest1Done = false;
    public bool quest2Done = false;
    public bool quest3Done = false;
    public bool quest4Done = false;

    // Сколько квестов завершено (0-4), определяет стадию дерева
    public int QuestsCompleted()
    {
        int count = 0;
        if (quest1Done) count++;
        if (quest2Done) count++;
        if (quest3Done) count++;
        if (quest4Done) count++;
        return count;
    }

    // Отметить квест выполненным и сохранить на диск
    public void SetQuestDone(int number)
    {
        switch (number)
        {
            case 1: quest1Done = true; break;
            case 2: quest2Done = true; break;
            case 3: quest3Done = true; break;
            case 4: quest4Done = true; break;
        }
        PlayerPrefs.SetInt("quest" + number, 1);
        PlayerPrefs.Save();
        Debug.Log($"[Прогресс] Квест {number} пройден. Всего: {QuestsCompleted()}/4");
    }

    // Подстраховка: сохраняем при сворачивании и выходе из игры
    private void OnApplicationPause(bool paused) { if (paused) PlayerPrefs.Save(); }
    private void OnApplicationQuit() { PlayerPrefs.Save(); }

    // Полный сброс прогресса для «Новой игры».
    // Стирает только игровые ключи — громкость и язык остаются.
    public static void ResetProgress()
    {
        for (int i = 1; i <= 4; i++)
            PlayerPrefs.DeleteKey("quest" + i);

        // просмотренные катсцены и разговоры
        foreach (var key in new[]
        {
            "cutscene_q1_intro", "cutscene_q2_intro", "cutscene_q3_intro", "cutscene_q4_intro",
            "dlg_hub_bird_intro", "dlg_hub_bird_intro_final"
        })
            PlayerPrefs.DeleteKey(key);

        // сделанные в диалогах выборы
        foreach (var key in new[]
        {
            "intro_fail", "intro_smalltree", "q1_doubt", "q1_simple",
            "q2_silent", "q2_admit", "q3_wrongwords", "q3_try",
            "q4_dontknow", "q4_waste",
            "a_sad_listen", "a_sad_fix", "a_anger_listen", "a_anger_fix",
            "a_tired_listen", "a_tired_fix", "a_fear_listen", "a_fear_fix",
            "a_hurt_listen", "a_hurt_fix"
        })
            PlayerPrefs.DeleteKey("choice_" + key);

        PlayerPrefs.Save();

        if (Instance != null)
        {
            Instance.quest1Done = Instance.quest2Done =
            Instance.quest3Done = Instance.quest4Done = false;
        }
    }

    // Полный сброс при выходе в главное меню: прогресс, имя игрока
    // и запомненные точки появления. Громкость и язык сохраняются.
    public static void ResetForNewGame()
    {
        ResetProgress();

        PlayerPrefs.DeleteKey("player_name");
        PlayerPrefs.DeleteKey("game_started");
        PlayerPrefs.Save();

        HeroSpawn.ClearSaved();
    }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Загружаем сохранённый прогресс
        quest1Done = PlayerPrefs.GetInt("quest1", 0) == 1;
        quest2Done = PlayerPrefs.GetInt("quest2", 0) == 1;
        quest3Done = PlayerPrefs.GetInt("quest3", 0) == 1;
        quest4Done = PlayerPrefs.GetInt("quest4", 0) == 1;
    }
}
