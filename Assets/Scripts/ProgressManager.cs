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
