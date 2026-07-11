using UnityEngine;

// Клик по птице в сцене Q1. Повесить на птицу вместе с Collider.
// До конца вступительной катсцены клики не работают.
// До выполнения квеста — beforeQuestDialogue,
// после квеста (и финального диалога) — afterQuestDialogue.
public class BirdInteractionQ1 : MonoBehaviour
{
    public DialogueData beforeQuestDialogue; // "Look at that beautiful sun..."
    public DialogueData afterQuestDialogue;  // "I think you're ready..."

    private void OnMouseDown()
    {
        var dm = DialogueManager.Instance;
        if (dm == null || dm.IsDialogueActive) return; // диалог идёт — не прерываем
        if (!Q1Cutscene.IntroFinished) return;         // интро ещё не закончилось

        bool questDone = ProgressManager.Instance != null &&
                         ProgressManager.Instance.quest1Done;

        DialogueData d = questDone ? afterQuestDialogue : beforeQuestDialogue;
        if (d != null) dm.StartDialogue(d);
    }
}
