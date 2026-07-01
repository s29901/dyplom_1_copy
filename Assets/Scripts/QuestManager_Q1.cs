using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class QuestManager_Q1 : MonoBehaviour
{
    [SerializeField] private Transform sun;
    [SerializeField] private Transform plant;
    [SerializeField] private WarmthBar warmthBar;
    [SerializeField] private HeroMovement heroMovement;   // bohater
    [SerializeField] private GameObject completionText;   // tekst "Pęd poczuł ciepło"

    [SerializeField] private float fillDuration = 20f;
    [SerializeField] private float sunRadius = 2f;

    private float warmth = 0f;
    private bool questDone = false;

    void Update()
    {
        if (questDone) return;

        float distance = Vector2.Distance(
            new Vector2(sun.position.x, sun.position.z),
            new Vector2(plant.position.x, plant.position.z)
        );

        if (distance < sunRadius)
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
        // Zatrzymujemy bohatera
        heroMovement.enabled = false;

        // Roślina rośnie, płynnie się zwiększa
        float elapsed = 0f;
        Vector3 startScale = plant.localScale;
        Vector3 endScale = startScale * 1.5f;

        while (elapsed < 1.5f)
        {
            elapsed += Time.deltaTime;
            plant.localScale = Vector3.Lerp(startScale, endScale, elapsed / 1.5f);
            yield return null;
        }

        // Pokazujemy tekst
        completionText.SetActive(true);

        // Czekamy 2 sekundy
        yield return new WaitForSeconds(2f);

        // Chowamy tekst, oddajemy kontrolę
        completionText.SetActive(false);
        heroMovement.enabled = true;
    }
}