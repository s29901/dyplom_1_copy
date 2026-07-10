using System.Collections.Generic;
using UnityEngine;

// Повесить на объект NPC вместе с Collider2D или Collider.
// Первый разговор — вступительный диалог, дальше — короткая повторная фраза.
// Пока идёт любой диалог, клики по NPC игнорируются (нельзя прервать).
// Подсказка "Поговорить" (talkPrompt):
//   - до первого разговора висит всегда;
//   - во время диалога скрыта;
//   - после первого разговора появляется, только когда герой рядом.
public class BirdInteraction : MonoBehaviour
{
    public DialogueData dialogue;        // первый диалог (вступление)
    public DialogueData repeatDialogue;  // повторная короткая фраза
    public string dialogueId = "hub_bird_intro"; // уникальный ключ этого NPC

    [Header("Кнопка 'Поговорить'")]
    public GameObject talkPrompt;   // объект-подсказка (дочерний объект птицы)
    public bool showAfterIntro = false; // показывать ли подсказку после интро (при подходе)
    public Transform hero;          // герой, для расстояния
    public float promptRadius = 4f; // на каком расстоянии показывать после интро

    // static — переживает перезагрузку сцены, сбрасывается при выходе из игры
    private static readonly HashSet<string> played = new HashSet<string>();

    private void Update()
    {
        if (talkPrompt == null) return;

        bool dialogueActive = DialogueManager.Instance != null &&
                              DialogueManager.Instance.IsDialogueActive;

        bool show;
        if (dialogueActive)
        {
            show = false; // во время диалога подсказку прячем
        }
        else if (!played.Contains(dialogueId))
        {
            show = true; // интро ещё не было — приглашаем поговорить
        }
        else if (!showAfterIntro)
        {
            show = false; // после интро подсказка выключена совсем
        }
        else
        {
            // После интро — только когда герой рядом (по X/Z)
            show = hero != null && HorizontalDistance(hero.position, transform.position) <= promptRadius;
        }

        if (talkPrompt.activeSelf != show)
            talkPrompt.SetActive(show);
    }

    private static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        return Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    }

    private void OnMouseDown()
    {
        StartTalk();
    }

    // Можно вызывать и с UI-кнопки (OnClick)
    public void StartTalk()
    {
        if (DialogueManager.Instance == null) return;

        // Идущий диалог прерывать нельзя
        if (DialogueManager.Instance.IsDialogueActive) return;

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
