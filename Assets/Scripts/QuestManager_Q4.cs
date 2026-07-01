using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestManager_Q4 : MonoBehaviour
{
    [Header("UI")]
    public GameObject completionText;

    [Header("Настройки")]
    public float questDuration = 30f;

    private float timer = 0f;
    private bool completed = false;

    void Start()
    {
        if (completionText != null)
            completionText.SetActive(false);
    }

    void Update()
    {
        if (completed) return;

        timer += Time.deltaTime;

        if (timer >= questDuration)
        {
            completed = true;
            StartCoroutine(CompleteQuest());
        }
    }

    System.Collections.IEnumerator CompleteQuest()
    {
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.quest4Done = true;

        if (completionText != null)
            completionText.SetActive(true);

        yield return new WaitForSeconds(3f);

        if (completionText != null)
            completionText.SetActive(false);
    }
}