using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

// Инструмент: Tools -> Replace TMP Fonts
// Меняет Font Asset у всех TMP-текстов в открытой сцене одним кликом.
public class ReplaceTMPFonts : EditorWindow
{
    private TMP_FontAsset newFont;

    [MenuItem("Tools/Replace TMP Fonts")]
    private static void Open()
    {
        GetWindow<ReplaceTMPFonts>("Replace TMP Fonts");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Новый шрифт (TMP Font Asset):");
        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField(newFont, typeof(TMP_FontAsset), false);

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(newFont == null))
        {
            if (GUILayout.Button("Заменить во всех текстах открытой сцены"))
            {
                ReplaceInOpenScenes();
            }
        }

        EditorGUILayout.HelpBox(
            "Находит все TMP-тексты в открытой сцене (включая выключенные) " +
            "и ставит им выбранный шрифт. Не забудь потом сохранить сцену (Cmd+S). " +
            "Повтори для каждой сцены и открытых префабов.", MessageType.Info);
    }

    private void ReplaceInOpenScenes()
    {
        // FindObjectsInactive.Include — берём и выключенные объекты (панели и т.п.)
        TMP_Text[] texts = Object.FindObjectsByType<TMP_Text>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        int count = 0;
        foreach (var t in texts)
        {
            if (t.font == newFont) continue;
            Undo.RecordObject(t, "Replace TMP Font");
            t.font = newFont;
            EditorUtility.SetDirty(t);
            count++;
        }

        if (count > 0)
            EditorSceneManager.MarkAllScenesDirty();

        Debug.Log($"[ReplaceTMPFonts] Заменён шрифт у {count} текстов. Сохрани сцену (Cmd+S).");
    }
}
