using UnityEngine;
using System.Collections;

public class QuestManager_Q2 : MonoBehaviour
{
    [SerializeField] private CloudDrag[] clouds;       // wszystkie 5 chmur
    [SerializeField] private HeroMovement heroMovement;
    [SerializeField] private GameObject completionText;
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
            ProgressManager.Instance.quest2Done = true;

        completionText.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        completionText.SetActive(false);

        if (heroMovement != null) heroMovement.enabled = true;
    }
}