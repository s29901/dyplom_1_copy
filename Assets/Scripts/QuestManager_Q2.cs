using UnityEngine;
using System.Collections;

public class QuestManager_Q2 : MonoBehaviour
{
    [SerializeField] private CloudDrag[] clouds;       // все 5 облаков
    [SerializeField] private HeroMovement heroMovement;
    [SerializeField] private GameObject completionText;      // запасной текст (не нужен, если задан диалог)
    [SerializeField] private DialogueData completionDialogue; // диалог после выполнения квеста
    [SerializeField] private Transform plant;

    private int cloudsCompleted = 0;

    void Start()
    {
        // Zapisujemy się na zdarzenie każdej chmury
        foreach (CloudDrag cloud in clouds)
            cloud.OnCloudDone += OnCloudCompleted;
    }

    void OnCloudCompleted()
    {
        cloudsCompleted++;
        StartCoroutine(PlantReact()); // drzewo ożywa

        if (cloudsCompleted >= 5)
            StartCoroutine(CompletionAnimation());
    }

    IEnumerator PlantReact()
    {
        // Drzewo lekko pulsuje
        Vector3 original = plant.localScale;
        Vector3 bigger = original * 1.1f;
        float elapsed = 0f;

        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            plant.localScale = Vector3.Lerp(original, bigger, elapsed / 0.3f);
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            plant.localScale = Vector3.Lerp(bigger, original, elapsed / 0.3f);
            yield return null;
        }
    }

    IEnumerator CompletionAnimation()
    {
        if (heroMovement != null) heroMovement.enabled = false;

        // Drzewo rośnie
        Vector3 startScale = plant.localScale;
        Vector3 endScale = startScale * 1.5f;
        float elapsed = 0f;
        while (elapsed < 1.5f)
        {
            elapsed += Time.deltaTime;
            plant.localScale = Vector3.Lerp(startScale, endScale, elapsed / 1.5f);
            yield return null;
        }

        if (ProgressManager.Instance != null)
            ProgressManager.Instance.SetQuestDone(2);

        // Ждём магию превращения дерева (искры + смена спрайта), если оно есть в сцене
        PlantGrowth plantGrowth = FindFirstObjectByType<PlantGrowth>();
        if (plantGrowth != null)
        {
            float waited = 0f;
            while (!plantGrowth.IsTransitioning && waited < 1f)
            {
                waited += Time.deltaTime;
                yield return null;
            }
            while (plantGrowth.IsTransitioning)
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
            yield return new WaitForSeconds(2.5f);
            completionText.SetActive(false);
        }

        if (heroMovement != null) heroMovement.enabled = true;
    }
}