using UnityEngine;

// Клик по птице в квестовой сцене. Повесить на птицу вместе с Collider.
// До конца вступительной катсцены клики не работают.
// До выполнения квеста — beforeQuestDialogue, после — afterQuestDialogue.
public class QuestBirdInteraction : MonoBehaviour
{
    public QuestCutscene cutscene;  // катсцена этой сцены (чтобы не мешать интро)
    public int questNumber = 2;     // номер квеста этой сцены (1-4)
    public DialogueData beforeQuestDialogue;
    public DialogueData afterQuestDialogue;

    private void OnMouseDown()
    {
        var dm = DialogueManager.Instance;
        if (dm == null || dm.IsDialogueActive) return;          // диалог идёт — не прерываем
        if (cutscene != null && !cutscene.IntroFinished) return; // интро ещё не закончилось

        DialogueData d = IsQuestDone() ? afterQuestDialogue : beforeQuestDialogue;
        if (d != null) dm.StartDialogue(d);
    }

    private bool IsQuestDone()
    {
        var pm = ProgressManager.Instance;
        if (pm == null) return false;
        switch (questNumber)
        {
            case 1: return pm.quest1Done;
            case 2: return pm.quest2Done;
            case 3: return pm.quest3Done;
            case 4: return pm.quest4Done;
            default: return false;
        }
    }
}
