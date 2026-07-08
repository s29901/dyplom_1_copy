using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject dialoguePanel;   // корневой объект окна диалога
    public TMP_Text nameText;          // текст с именем говорящего (плашка "ELDROS")
    public TMP_Text dialogueText;      // основной текст реплики
    public Image portraitImage;        // портрет персонажа справа/слева

    [Header("Позиции слева/справа")]
    // Пустые RectTransform-метки внутри того же Canvas/панели, куда нужно
    // "переставлять" nameText и portraitImage в зависимости от говорящего.
    // Создай 4 пустых UI-объекта (без Image, просто RectTransform) и расставь
    // их там, где должны быть имя/портрет героя (слева) и птицы (справа).
    public RectTransform leftNameAnchor;
    public RectTransform leftPortraitAnchor;
    public RectTransform rightNameAnchor;
    public RectTransform rightPortraitAnchor;

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

        // Переставляем имя и портрет на нужную сторону в зависимости от говорящего
        RectTransform nameAnchor = line.speakerOnRight ? rightNameAnchor : leftNameAnchor;
        RectTransform portraitAnchor = line.speakerOnRight ? rightPortraitAnchor : leftPortraitAnchor;

        if (nameAnchor != null && nameText != null)
        {
            nameText.rectTransform.anchoredPosition = nameAnchor.anchoredPosition;
        }

        if (portraitAnchor != null && portraitImage != null)
        {
            portraitImage.rectTransform.anchoredPosition = portraitAnchor.anchoredPosition;
        }

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
        // Простейший ввод для теста: ЛКМ или пробел продвигает диалог
        if (dialoguePanel.activeSelf &&
            (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            OnAdvancePressed();
        }
    }
}