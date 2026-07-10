using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class QuestManager_Q1 : MonoBehaviour
{
    [SerializeField] private Transform sun;
    [SerializeField] private WarmthBar warmthBar;
    [SerializeField] private HeroMovement heroMovement;   // bohater
    [SerializeField] private GameObject completionText;   // tekst "Pęd poczuł ciepło"

    [Header("Зона срабатывания (объект с WarmthZone)")]
    [SerializeField] private WarmthZone warmthZone;

    [SerializeField] private float fillDuration = 20f;

    private float warmth = 0f;
    private bool questDone = false;

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
            ProgressManager.Instance.quest1Done = true;

        StartCoroutine(CompletionAnimation());
    }

    IEnumerator CompletionAnimation()
    {
        // Останавливаем героя
        heroMovement.enabled = false;

        // Дерево само переключится на следующую стадию (PlantGrowth),
        // даём секунду полюбоваться
        yield return new WaitForSeconds(1f);

        // Показываем текст
        completionText.SetActive(true);

        // Czekamy 2 sekundy
        yield return new WaitForSeconds(2f);

        // Chowamy tekst, oddajemy kontrolę
        completionText.SetActive(false);
        heroMovement.enabled = true;
    }
}