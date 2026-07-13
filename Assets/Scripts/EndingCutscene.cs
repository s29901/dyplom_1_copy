using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Финальная сцена (99_Ending):
// проигрывает EndingDialogue и (опционально) возвращает в главное меню.
// Повесить на пустой объект. В сцене должен быть DialogueManager с UI
// (скопировать объект DialogueUI/DialogueManager из Q1 или Q2).
public class EndingCutscene : MonoBehaviour
{
    [Header("Диалог")]
    public DialogueData endingDialogue;
    public float delayBeforeDialogue = 1.5f; // пауза, чтобы игрок осмотрел сад

    [Header("После диалога")]
    public string nextScene = "";          // например "00_MainMenu"; пусто = остаться
    public float delayAfterDialogue = 3f;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(delayBeforeDialogue);

        if (endingDialogue != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(endingDialogue);
            while (DialogueManager.Instance.IsDialogueActive)
                yield return null;
        }

        if (!string.IsNullOrEmpty(nextScene))
        {
            yield return new WaitForSeconds(delayAfterDialogue);
            SceneManager.LoadScene(nextScene);
        }
    }
}
