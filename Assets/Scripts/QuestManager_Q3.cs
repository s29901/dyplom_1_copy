using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class QuestManager_Q3 : MonoBehaviour
{
    [Header("Настройки")]
    public int totalBubbles = 4;

    [Header("UI")]
    public GameObject completionText; // запасной текст (не нужен, если задан диалог)

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
    }

    public void OnBubbleSaid()
    {
        saidCount++;
        if (saidCount >= totalBubbles)
        {
            StartCoroutine(CompleteQuest());
        }
    }

    System.Collections.IEnumerator CompleteQuest()
    {
        if (heroMovement != null) heroMovement.enabled = false;

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
            // Финальный диалог запускается сам
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