using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class QuestManager_Q3 : MonoBehaviour
{
    [Header("Настройки")]
    public int totalBubbles = 5; // сколько зверьков нужно выслушать

    [Header("UI")]
    public GameObject completionText;  // запасной текст (не нужен, если задан диалог)
    public TMP_Text counterText;       // счётчик "3 / 5" (объект с именем AnimalCounter)
    public string counterFormat = "{0} / {1}";

    [Header("Диалог после квеста")]
    public DialogueData completionDialogue;

    [Header("Растение")]
    public PlantGrowth plant;

    [Header("Герой")]
    public HeroMovement heroMovement;

    private int saidCount = 0;

    void Start()
    {
        if (completionText != null)
            completionText.SetActive(false);

        // авто-поиск счётчика по имени, если не назначен
        if (counterText == null)
        {
            foreach (var t in FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (t.name.Trim() == "AnimalCounter") { counterText = t; break; }
        }
        UpdateCounter();
    }

    // Новый путь: зверёк выслушан
    public void OnAnimalHeard() => Progress();

    // Старый путь: пузырь озвучен (оставлено для совместимости)
    public void OnBubbleSaid() => Progress();

    private void Progress()
    {
        saidCount++;
        UpdateCounter();
        if (saidCount >= totalBubbles)
            StartCoroutine(CompleteQuest());
    }

    private void UpdateCounter()
    {
        if (counterText != null)
            counterText.text = string.Format(counterFormat, saidCount, totalBubbles);
    }

    System.Collections.IEnumerator CompleteQuest()
    {
        if (heroMovement != null) heroMovement.enabled = false;

        // счётчик выполнил свою задачу — мягко убираем
        if (counterText != null) counterText.gameObject.SetActive(false);

        yield return new WaitForSeconds(1f);

        if (ProgressManager.Instance != null)
            ProgressManager.Instance.SetQuestDone(3);

        // Ждём магию превращения дерева (искры + смена спрайта)
        if (plant == null) plant = FindFirstObjectByType<PlantGrowth>();
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

        if (heroMovement != null) heroMovement.enabled = true;
    }
}
