using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class QuestManager_Q3 : MonoBehaviour
{
    [Header("Настройки")]
    public int totalBubbles = 4;

    [Header("UI")]
    public GameObject completionText;

    [Header("Растение")]
    public PlantGrowth plant;

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
        yield return new WaitForSeconds(1f);

        if (ProgressManager.Instance != null)
            ProgressManager.Instance.quest3Done = true;

        if (plant != null)
            plant.UpdatePlant();

        if (completionText != null)
            completionText.SetActive(true);

        // tekst jest wyświetlany 3 sekundy, potem zanika
        yield return new WaitForSeconds(3f);
    
        if (completionText != null)
            completionText.SetActive(false);
    }
}