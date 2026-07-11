using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class QuestManager_Q1 : MonoBehaviour
{
    [SerializeField] private Transform sun;
    [SerializeField] private WarmthBar warmthBar;
    [SerializeField] private HeroMovement heroMovement;   // bohater
    [SerializeField] private GameObject completionText;   // текст-заглушка (не нужен, если задан диалог)
    [SerializeField] private DialogueData completionDialogue; // диалог после выполнения квеста

    [Header("Зона срабатывания (объект с WarmthZone)")]
    [SerializeField] private WarmthZone warmthZone;

    [SerializeField] private float fillDuration = 20f;

    private float warmth = 0f;
    private bool questDone = false;

    void Start()
    {
        // Квест уже пройден ранее — не запускаем его заново и прячем шкалу
        if (ProgressManager.Instance != null && ProgressManager.Instance.quest1Done)
        {
            questDone = true;
            if (warmthBar != null)
                warmthBar.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (questDone || warmthZone == null) return;

        if (warmthZone.Contains(sun.position))
        {
            warmth += Time.deltaTime / fillDuration;
            warmth = Mathf.Clamp01(warmth);
            warmthBar.SetFill(warmth);

            if (warmth >= 1f)
                CompleteQuest();
        }
    }

    void CompleteQuest()
    {
        questDone = true;

        if (ProgressManager.Instance != null)
            ProgressManager.Instance.SetQuestDone(1);

        // Прячем шкалу тепла — она больше не нужна
        if (warmthBar != null)
            warmthBar.gameObject.SetActive(false);

        StartCoroutine(CompletionAnimation());
    }

    IEnumerator CompletionAnimation()
    {
        // Останавливаем героя
        heroMovement.enabled = false;

        // Ждём магию превращения дерева: искры, растворение, новый спрайт
        PlantGrowth plantGrowth = FindFirstObjectByType<PlantGrowth>();
        if (plantGrowth != null)
        {
            // даём превращению начаться (на случай задержки — ждём максимум 1 сек)
            float waited = 0f;
            while (!plantGrowth.IsTransitioning && waited < 1f)
            {
                waited += Time.deltaTime;
                yield return null;
            }

            // ждём конца превращения
            while (plantGrowth.IsTransitioning)
                yield return null;

            // короткая пауза, чтобы разглядеть новое дерево
            yield return new WaitForSeconds(0.8f);
        }
        else
        {
            yield return new WaitForSeconds(1f);
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
            // Запасной вариант: просто текст
            completionText.SetActive(true);
            yield return new WaitForSeconds(2f);
            completionText.SetActive(false);
        }

        heroMovement.enabled = true;
    }
}