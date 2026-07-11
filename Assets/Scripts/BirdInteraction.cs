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

    [Header("Блокировка героя до разговора")]
    public HeroMovement heroMovement; // герой не сможет ходить, пока не поговорит с птицей

    [Header("Кнопка 'Поговорить'")]
    public GameObject talkPrompt;   // объект-подсказка (дочерний объект птицы)
    public bool showAfterIntro = false; // показывать ли подсказку после интро (при подходе)
    public Transform hero;          // герой, для расстояния
    public float promptRadius = 4f; // на каком расстоянии показывать после интро

    // Хранится в PlayerPrefs — интро показывается один раз за всю игру,
    // даже между запусками. Сброс: Tools -> Reset Story Progress.
    public static bool WasPlayed(string id) => PlayerPrefs.GetInt("dlg_" + id, 0) == 1;

    public static void MarkPlayed(string id)
    {
        PlayerPrefs.SetInt("dlg_" + id, 1);
        PlayerPrefs.Save();
    }

    private void Update()
    {
        bool dialogueActive = DialogueManager.Instance != null &&
                              DialogueManager.Instance.IsDialogueActive;

        // Герой стоит на месте, пока не поговорил с птицей,
        // а также во время любого диалога
        if (heroMovement != null)
            heroMovement.enabled = WasPlayed(dialogueId) && !dialogueActive;

        if (talkPrompt == null) return;

        bool show;
        if (dialogueActive)
        {
            show = false; // во время диалога подсказку прячем
        }
        else if (!WasPlayed(dialogueId))
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

        if (!WasPlayed(dialogueId))
        {
            MarkPlayed(dialogueId);
            DialogueManager.Instance.StartDialogue(dialogue);
        }
        else if (repeatDialogue != null)
        {
            DialogueManager.Instance.StartDialogue(repeatDialogue);
        }
    }
}
