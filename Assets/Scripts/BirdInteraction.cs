using System.Collections.Generic;
using UnityEngine;

// Повесить на объект NPC вместе с Collider2D (Is Trigger можно оставить включённым).
// Первый клик запускает вступительный диалог, последующие — короткую повторную фразу.
public class BirdInteraction : MonoBehaviour
{
    public DialogueData dialogue;        // первый диалог (вступление)
    public DialogueData repeatDialogue;  // повторная короткая фраза
    public string dialogueId = "hub_bird_intro"; // уникальный ключ этого NPC

    // static — переживает перезагрузку сцены, сбрасывается при выходе из игры
    private static readonly HashSet<string> played = new HashSet<string>();

    private void OnMouseDown()
    {
        if (DialogueManager.Instance == null) return;

        if (!played.Contains(dialogueId))
        {
            played.Add(dialogueId);
            DialogueManager.Instance.StartDialogue(dialogue);
        }
        else if (repeatDialogue != null)
        {
            DialogueManager.Instance.StartDialogue(repeatDialogue);
        }
    }
}
