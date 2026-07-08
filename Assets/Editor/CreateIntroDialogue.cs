using UnityEditor;
using UnityEngine;

// Разместить в папке Assets/Editor/ (Unity требует, чтобы редакторские скрипты лежали там).
// После импорта: меню Dialogue -> Create Intro Garden Dialogue.
// Создаст готовый ассет IntroGardenDialogue.asset со всеми репликами.
// Портреты (speakerPortrait) останутся пустыми — просто перетащи спрайты
// птицы и героя на нужные строки прямо в инспекторе ассета.
public static class CreateIntroDialogue
{
    [MenuItem("Dialogue/Create Intro Garden Dialogue")]
    public static void Create()
    {
        var asset = ScriptableObject.CreateInstance<DialogueData>();

        var raw = new (string speaker, string text)[]
        {
            ("Bird", "Oh... you're finally here. I've been waiting for you."),
            ("Hero", "Where am I...? What is this place?"),
            ("Bird", "This is your inner garden."),
            ("Hero", "It's... so empty. And everything's in ruins."),
            ("Bird", "It wasn't always like this. The grown-ups who were meant to care for this place when you were little couldn't give it the care it needed."),
            ("Hero", "Is that why it looks like this?"),
            ("Bird", "Yes. Some plants withered away. Cracks spread through the ground. And some things never got the chance to grow at all."),
            ("Hero", "That's... really sad."),
            ("Bird", "It is. But there's something you should know."),
            ("Hero", "What?"),
            ("Bird", "You're not a child anymore. Now you're the one who can bring this garden back to life."),
            ("Hero", "Do you really think I can?"),
            ("Bird", "I know you can. It won't happen overnight, though. We'll have to explore the garden, uncover what happened in each part of it, and slowly help it heal."),
            ("Bird", "See that little tree? That's your inner tree. It's small now, but every challenge you overcome will help it grow stronger."),
            ("Hero", "So if I help the garden..."),
            ("Bird", "...the tree will grow too. And so will you."),
            ("Hero", "Then... let's do it."),
            ("Bird", "I'll be with you every step of the way. Come on."),
        };

        asset.lines = new DialogueData.DialogueLine[raw.Length];
        for (int i = 0; i < raw.Length; i++)
        {
            asset.lines[i] = new DialogueData.DialogueLine
            {
                speakerName = raw[i].speaker,
                text = raw[i].text,
                speakerOnRight = raw[i].speaker == "Bird" // птица справа, герой слева
            };
        }

        string path = "Assets/IntroGardenDialogue.asset";
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        EditorGUIUtility.PingObject(asset);
        Debug.Log("Intro dialogue created at " + path);
    }
}