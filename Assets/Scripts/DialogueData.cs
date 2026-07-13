using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;
        public Sprite speakerPortrait;
        public bool speakerOnRight; // true = имя/портрет справа (напр. птица), false = слева (напр. герой)

        [TextArea(2, 5)]
        public string text;

        // ------ Выбор игрока (опционально) ------
        // Если choiceA не пустой, после этой реплики появляются две кнопки.
        // Выбранный вариант произносит герой, затем говорящий этой реплики
        // отвечает reactionA/reactionB, и диалог продолжается дальше.
        [Header("Выбор игрока (пусто = нет выбора)")]
        public string choiceA;
        public string choiceB;
        public string choiceKeyA;   // ключ PlayerPrefs (для callback в финале), можно пусто
        public string choiceKeyB;
        [TextArea(2, 5)] public string reactionA;
        [TextArea(2, 5)] public string reactionB;

        // ------ Условная реплика (опционально) ------
        // Если задан ключ — реплика показывается только если игрок
        // когда-то сделал выбор с этим ключом (callback птицы в финале).
        [Header("Показывать только после выбора с ключом (пусто = всегда)")]
        public string requiredChoiceKey;

        public bool HasChoice => !string.IsNullOrEmpty(choiceA) && !string.IsNullOrEmpty(choiceB);
    }

    public DialogueLine[] lines;
}
