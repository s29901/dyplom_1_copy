using System.Collections;
using System.Collections.Generic;
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

    [Header("UI выбора (опционально; если не задано — авто-выбор A)")]
    public GameObject choicePanel;     // панель с двумя кнопками
    public TMP_Text choiceTextA;       // текст на кнопке A
    public TMP_Text choiceTextB;       // текст на кнопке B

    [Header("Позиции (anchoredPosition)")]
    public Vector2 namePosLeft = new Vector2(-955f, 1749f);      // имя героя
    public Vector2 namePosRight = new Vector2(1530f, 1616f);     // имя птицы
    public Vector2 portraitPosLeft = new Vector2(-1136f, 1788f); // портрет героя
    public Vector2 portraitPosRight = new Vector2(2062f, 3178f); // портрет птицы

    [Header("Размер портрета (Width/Height; 0,0 = не менять)")]
    public Vector2 portraitSizeLeft = Vector2.zero;  // размер рамки для героя
    public Vector2 portraitSizeRight = Vector2.zero; // размер рамки для птицы

    [Header("Settings")]
    public float typingSpeed = 0.03f;  // задержка между буквами (эффект печатной машинки)

    // Плейлист реплик текущего диалога (сюда вставляются реплики после выбора)
    private List<DialogueData.DialogueLine> playlist;
    private int currentLineIndex;
    private bool isTyping;
    private bool choiceActive;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        AutoWire(); // добираем не назначенные ссылки по именам объектов
        BindChoiceButtons();

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        else Debug.LogError("DialogueManager: не найден DialoguePanel в сцене!");
        if (choicePanel != null) choicePanel.SetActive(false);
    }

    // Префабы не могут ссылаться на объекты других префабов, поэтому
    // пустые ссылки заполняем поиском по имени (работает и для выключенных объектов).
    private void AutoWire()
    {
        if (dialoguePanel == null) dialoguePanel = FindByName("DialoguePanel")?.gameObject;
        if (nameText == null) nameText = FindByName("NameText")?.GetComponent<TMP_Text>();
        if (dialogueText == null) dialogueText = FindByName("DialogueText")?.GetComponent<TMP_Text>();
        if (portraitImage == null) portraitImage = FindByName("PortraitImage")?.GetComponent<Image>();
        if (choicePanel == null) choicePanel = FindByName("ChoicePanel")?.gameObject;
        if (choiceTextA == null) choiceTextA = FindByName("Choice Text A")?.GetComponentInChildren<TMP_Text>(true);
        if (choiceTextB == null) choiceTextB = FindByName("Choice Text B")?.GetComponentInChildren<TMP_Text>(true);
    }

    // Кнопки выбора подписываем из кода — не нужно настраивать OnClick в редакторе
    private void BindChoiceButtons()
    {
        BindButton("Choice Text A", OnChoiceA);
        BindButton("Choice Text B", OnChoiceB);
    }

    private RectTransform choiceRectA, choiceRectB; // для ручной проверки клика

    private void BindButton(string objName, UnityEngine.Events.UnityAction action)
    {
        var t = FindByName(objName);
        if (t == null) return;

        var rect = t.GetComponent<RectTransform>();
        if (objName.EndsWith("A")) choiceRectA = rect; else choiceRectB = rect;

        var btn = t.GetComponent<Button>() ?? t.GetComponentInChildren<Button>(true);
        if (btn == null) return;
        btn.onClick.RemoveListener(action);
        btn.onClick.AddListener(action);
    }

    // Попадает ли курсор в прямоугольник кнопки (не зависит от EventSystem/Raycast)
    private bool PointerOver(RectTransform rt)
    {
        if (rt == null) return false;
        var canvas = rt.GetComponentInParent<Canvas>();
        Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? canvas.worldCamera : null;
        return RectTransformUtility.RectangleContainsScreenPoint(rt, Input.mousePosition, cam);
    }

    private static Transform FindByName(string n)
    {
        foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (t.name == n) return t;
        return null;
    }

    public void StartDialogue(DialogueData dialogue)
    {
        playlist = new List<DialogueData.DialogueLine>();
        foreach (var l in dialogue.lines)
        {
            // условные реплики: пропускаем, если выбор не был сделан
            if (!string.IsNullOrEmpty(l.requiredChoiceKey) && !ChoiceMade(l.requiredChoiceKey))
                continue;
            playlist.Add(l);
        }
        if (playlist.Count == 0) return;

        currentLineIndex = 0;
        startFrame = Time.frameCount;
        isTyping = false;
        choiceActive = false;
        dialoguePanel.SetActive(true);
        ShowLine();
    }

    // Имя игрока, введённое в главном меню
    public static string PlayerName => PlayerPrefs.GetString("player_name", "Hero");

    // Был ли сделан выбор с данным ключом (для callback-реплик)
    public static bool ChoiceMade(string key) =>
        PlayerPrefs.GetInt("choice_" + key, 0) == 1;

    // Подстановка имени: {name} внутри текста заменяется на имя игрока
    private static string ProcessText(string t) => t.Replace("{name}", PlayerName);

    private void ShowLine()
    {
        var line = playlist[currentLineIndex];
        nameText.text = line.speakerName == "Hero" ? PlayerName : line.speakerName;

        if (portraitImage != null)
        {
            portraitImage.sprite = line.speakerPortrait;
            portraitImage.enabled = line.speakerPortrait != null;
        }

        Vector2 namePos = line.speakerOnRight ? namePosRight : namePosLeft;
        Vector2 portraitPos = line.speakerOnRight ? portraitPosRight : portraitPosLeft;

        if (nameText != null)
        {
            SetSideAnchor(nameText.rectTransform, line.speakerOnRight);
            nameText.rectTransform.anchoredPosition = namePos;
        }
        if (portraitImage != null)
        {
            SetSideAnchor(portraitImage.rectTransform, line.speakerOnRight);
            portraitImage.rectTransform.anchoredPosition = portraitPos;
            Vector2 size = line.speakerOnRight ? portraitSizeRight : portraitSizeLeft;
            if (size != Vector2.zero)
                portraitImage.rectTransform.sizeDelta = size;
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(ProcessText(line.text), line));
    }

    // Прибивает элемент к нижнему левому или нижнему правому углу родителя.
    // Благодаря этому позиции (anchoredPosition) — небольшие отступы от угла,
    // одинаково работающие на любом разрешении и соотношении сторон.
    private static void SetSideAnchor(RectTransform rt, bool right)
    {
        Vector2 corner = right ? new Vector2(1f, 0f) : new Vector2(0f, 0f);
        rt.anchorMin = corner;
        rt.anchorMax = corner;
        rt.pivot = corner;
    }

    private IEnumerator TypeText(string text, DialogueData.DialogueLine line)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;

        if (line.HasChoice)
            ShowChoices(line);
    }

    private void ShowChoices(DialogueData.DialogueLine line)
    {
        if (choicePanel == null || choiceTextA == null || choiceTextB == null)
        {
            // UI выбора не настроен — тихо выбираем вариант A, чтобы игра не встала
            ApplyChoice(line, true);
            return;
        }
        choiceActive = true;
        choiceTextA.text = ProcessText(line.choiceA);
        choiceTextB.text = ProcessText(line.choiceB);
        choicePanel.SetActive(true);
    }

    // Повесить на кнопки A и B (OnClick)
    public void OnChoiceA() => SelectChoice(true);
    public void OnChoiceB() => SelectChoice(false);

    private void SelectChoice(bool a)
    {
        if (!choiceActive) return; // защита от двойного срабатывания (клик + onClick)
        var line = playlist[currentLineIndex];
        if (!line.HasChoice) return;

        choiceActive = false;
        if (choicePanel != null) choicePanel.SetActive(false);
        ApplyChoice(line, a);
        // ApplyChoice вставил реплики после текущей — показываем их
        currentLineIndex++;
        ShowLine();
    }

    // Вставляет в плейлист реплику героя (выбранный вариант) и ответ собеседника
    private void ApplyChoice(DialogueData.DialogueLine line, bool a)
    {
        string chosen = a ? line.choiceA : line.choiceB;
        string key = a ? line.choiceKeyA : line.choiceKeyB;
        string reaction = a ? line.reactionA : line.reactionB;

        if (!string.IsNullOrEmpty(key))
            PlayerPrefs.SetInt("choice_" + key, 1);

        var heroLine = new DialogueData.DialogueLine
        {
            speakerName = "Hero",
            speakerPortrait = FindHeroPortrait(),
            speakerOnRight = false,
            text = chosen
        };
        playlist.Insert(currentLineIndex + 1, heroLine);

        if (!string.IsNullOrEmpty(reaction))
        {
            var reactionLine = new DialogueData.DialogueLine
            {
                speakerName = line.speakerName,
                speakerPortrait = line.speakerPortrait,
                speakerOnRight = line.speakerOnRight,
                text = reaction
            };
            playlist.Insert(currentLineIndex + 2, reactionLine);
        }
    }

    // Портрет героя берём из любой его реплики в этом диалоге
    private Sprite FindHeroPortrait()
    {
        foreach (var l in playlist)
            if (!l.speakerOnRight && l.speakerPortrait != null)
                return l.speakerPortrait;
        return null;
    }

    // Вызывать по клику/кнопке "продолжить"
    public void OnAdvancePressed()
    {
        if (playlist == null || !IsDialogueActive) return;
        if (choiceActive) return; // ждём нажатия кнопки выбора

        var line = playlist[currentLineIndex];

        if (isTyping)
        {
            // Долистать текст мгновенно, если он ещё печатается
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            dialogueText.text = ProcessText(line.text);
            isTyping = false;
            if (line.HasChoice) ShowChoices(line);
            return;
        }

        if (line.HasChoice) return; // выбор уже показан кнопками

        currentLineIndex++;
        if (currentLineIndex >= playlist.Count)
        {
            EndDialogue();
        }
        else
        {
            ShowLine();
        }
    }

    // Событие: диалог закончился (для катсцен и квестов)
    public System.Action OnDialogueEnded;

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
        playlist = null;
        OnDialogueEnded?.Invoke();
    }

    private void Update()
    {
        if (dialoguePanel == null || !dialoguePanel.activeSelf) return;

        // Момент выбора: клавиши 1/2 или клик прямо по прямоугольнику кнопки.
        // Ручная проверка не зависит от EventSystem и Raycast-настроек сцены.
        if (choiceActive)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) { SelectChoice(true); return; }
            if (Input.GetKeyDown(KeyCode.Alpha2)) { SelectChoice(false); return; }
            if (Input.GetMouseButtonDown(0))
            {
                if (PointerOver(choiceRectA)) SelectChoice(true);
                else if (PointerOver(choiceRectB)) SelectChoice(false);
            }
            return;
        }

        // ЛКМ или пробел продвигает диалог.
        if (Time.frameCount != startFrame &&
            (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            OnAdvancePressed();
        }
    }
}
