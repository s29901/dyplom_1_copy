using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    // Идёт ли сейчас диалог (для блокировки повторных кликов по NPC и т.п.)
    public bool IsDialogueActive => dialoguePanel != null && dialoguePanel.activeSelf;

    private int startFrame; // кадр, в котором диалог начался

    [Header("UI References")]
    public GameObject dialoguePanel;   // корневой объект окна диалога
    public TMP_Text nameText;          // текст с именем говорящего (плашка "ELDROS")
    public TMP_Text dialogueText;      // основной текст реплики
    public Image portraitImage;        // портрет персонажа справа/слева

    [Header("Позиции (anchoredPosition)")]
    public Vector2 namePosLeft = new Vector2(-955f, 1749f);      // имя героя
    public Vector2 namePosRight = new Vector2(1530f, 1616f);     // имя птицы
    public Vector2 portraitPosLeft = new Vector2(-1136f, 1788f); // портрет героя
    public Vector2 portraitPosRight = new Vector2(2062f, 3178f); // портрет птицы

    [Header("Settings")]
    public float typingSpeed = 0.03f;  // задержка между буквами (эффект печатной машинки)

    private DialogueData currentDialogue;
    private int currentLineIndex;
    private bool isTyping;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(DialogueData dialogue)
    {
        currentDialogue = dialogue;
        currentLineIndex = 0;
        startFrame = Time.frameCount;
        dialoguePanel.SetActive(true);
        ShowLine();
    }

    private void ShowLine()
    {
        var line = currentDialogue.lines[currentLineIndex];
        nameText.text = line.speakerName;

        if (portraitImage != null)
        {
            portraitImage.sprite = line.speakerPortrait;
            portraitImage.enabled = line.speakerPortrait != null;
        }

        Vector2 namePos = line.speakerOnRight ? namePosRight : namePosLeft;
        Vector2 portraitPos = line.speakerOnRight ? portraitPosRight : portraitPosLeft;

        if (nameText != null) nameText.rectTransform.anchoredPosition = namePos;
        if (portraitImage != null) portraitImage.rectTransform.anchoredPosition = portraitPos;

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(line.text));
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    // Вызывать по клику/кнопке "продолжить"
    public void OnAdvancePressed()
    {
        if (currentDialogue == null) return;

        if (isTyping)
        {
            // Долистать текст мгновенно, если он ещё печатается
            StopCoroutine(typingCoroutine);
            dialogueText.text = currentDialogue.lines[currentLineIndex].text;
            isTyping = false;
            return;
        }

        currentLineIndex++;
        if (currentLineIndex >= currentDialogue.lines.Length)
        {
            EndDialogue();
        }
        else
        {
            ShowLine();
        }
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        currentDialogue = null;
    }

    private void Update()
    {
        // ЛКМ или пробел продвигает диалог.
        // Кадр старта пропускаем, чтобы клик, запустивший диалог,
        // не пролистнул сразу первую реплику.
        if (dialoguePanel.activeSelf && Time.frameCount != startFrame &&
            (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            OnAdvancePressed();
        }
    }
}