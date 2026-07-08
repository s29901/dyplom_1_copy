using UnityEngine;

// Повесить на объект птицы вместе с Collider2D (Is Trigger можно оставить включённым).
// Диалог стартует по клику мышью в любой момент — не важно, летит птица или сидит на ветке.
public class BirdInteraction : MonoBehaviour
{
    public DialogueData dialogue;

    private void OnMouseDown()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(dialogue);
        }
    }
}