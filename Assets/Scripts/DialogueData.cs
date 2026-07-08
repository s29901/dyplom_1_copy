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
    }
 
    public DialogueLine[] lines;
}