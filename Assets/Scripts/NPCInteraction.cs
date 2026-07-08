using UnityEngine;

// Повесить на NPC. Нужен Collider2D с "Is Trigger" и тег "Player" на игроке.
public class NPCInteraction : MonoBehaviour
{
    public DialogueData dialogue;
    public KeyCode interactKey = KeyCode.E;

    private bool playerInRange;

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            DialogueManager.Instance.StartDialogue(dialogue);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}