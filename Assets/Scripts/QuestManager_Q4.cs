using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class QuestManager_Q4 : MonoBehaviour
{
    [Header("UI")]
    public GameObject completionText;

    [Header("Отладочный таймер (выключить перед релизом)")]
    public bool showDebugTimer = true;
    public TMP_Text timerText; // можно не назначать: найдётся объект "QuestTimer"

    [Header("Настройки")]
    public float questDuration = 30f;

    [Header("Диалоги (нужен DialogueManager в сцене)")]
    public DialogueData introDialogue;      // играет при входе в сцену
    public DialogueData completionDialogue; // играет после questDuration

    private float timer = 0f;
    private bool completed = false;
    private bool introDone = false;

    System.Collections.IEnumerator Start()
    {
        if (completionText != null)
            completionText.SetActive(false);

        if (timerText == null)
        {
            foreach (var t in FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (t.name.Trim() == "QuestTimer") { timerText = t; break; }
            if (timerText == null)
                Debug.LogWarning("QuestManager_Q4: не найден TMP-текст 'QuestTimer'");
        }
        if (timerText != null)
            timerText.gameObject.SetActive(showDebugTimer);

        yield return null; // кадр на инициализацию DialogueManager

        if (introDialogue != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(introDialogue);
            while (DialogueManager.Instance.IsDialogueActive)
                yield return null;
        }
        introDone = true;
    }

    void Update()
    {
        if (completed || !introDone) { UpdateTimerLabel(true); return; }

        // пока открыт диалог — «отдых» не отсчитывается
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            UpdateTimerLabel(true);
            return;
        }

        timer += Time.deltaTime;
        UpdateTimerLabel(false);

        if (timer >= questDuration)
        {
            completed = true;
            if (timerText != null) timerText.gameObject.SetActive(false);
            StartCoroutine(CompleteQuest());
        }
    }

    // Отладочная строка: сколько осталось; "⏸" — таймер стоит (диалог/интро)
    private void UpdateTimerLabel(bool paused)
    {
        if (!showDebugTimer || timerText == null) return;
        float left = Mathf.Max(0f, questDuration - timer);
        timerText.text = $"Q4: {left:0.0}s{(paused ? " (pause)" : "")}";
    }

    System.Collections.IEnumerator CompleteQuest()
    {
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.SetQuestDone(4);

        // Если в сцене есть дерево — ждём его магическое превращение
        var plant = FindFirstObjectByType<PlantGrowth>();
        if (plant != null)
        {
            float waited = 0f;
            while (!plant.IsTransitioning && waited < 1f)
            {
                waited += Time.deltaTime;
                yield return null;
            }
            while (plant.IsTransitioning)
                yield return null;

            yield return new WaitForSeconds(0.8f);
        }

        if (completionDialogue != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(completionDialogue);
            while (DialogueManager.Instance.IsDialogueActive)
                yield return null;
        }
        else if (completionText != null)
        {
            completionText.SetActive(true);
            yield return new WaitForSeconds(3f);
            completionText.SetActive(false);
        }
    }
}
