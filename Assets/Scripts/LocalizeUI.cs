using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Перевод надписей интерфейса на польский.
//
// Ничего настраивать не нужно: объект создаётся сам, при загрузке каждой сцены
// обходит все TMP-тексты и подставляет перевод из Resources/localization/ui_pl.json.
// Английский оригинал запоминается, поэтому переключение языка работает в обе стороны.
public class LocalizeUI : MonoBehaviour
{
    private static LocalizeUI instance;

    // Оригинальный (английский) текст каждого поля
    private readonly Dictionary<TMP_Text, string> originals = new Dictionary<TMP_Text, string>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (instance != null) return;
        var go = new GameObject("~LocalizeUI");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<LocalizeUI>();
        instance.Apply();

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += (s, m) => instance.Apply();
        Loc.OnLanguageChanged += () => instance.Apply();
    }

    // Пройтись по всем текстам сцены и выставить нужный язык
    public void Apply()
    {
        // чистим ссылки на уничтоженные объекты
        var dead = new List<TMP_Text>();
        foreach (var kv in originals)
            if (kv.Key == null) dead.Add(kv.Key);
        foreach (var d in dead) originals.Remove(d);

        foreach (var t in FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t == null) continue;

            // Диалоги переводятся отдельно (через DialogueData), их не трогаем
            if (IsDialogueText(t)) continue;

            if (!originals.TryGetValue(t, out var en))
            {
                en = t.text;
                originals[t] = en;
            }

            string translated = Loc.UI(en);
            if (t.text != translated) t.text = translated;
        }
    }

    private static bool IsDialogueText(TMP_Text t)
    {
        string n = t.name;
        if (n == "DialogueText" || n == "NameText") return true;
        if (n.StartsWith("tmp_Choice")) return true;

        var p = t.transform.parent;
        while (p != null)
        {
            if (p.name == "DialoguePanel" || p.name == "ChoicePanel") return true;
            p = p.parent;
        }
        return false;
    }
}
