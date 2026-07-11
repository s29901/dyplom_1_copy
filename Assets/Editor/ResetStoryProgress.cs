using UnityEngine;
using UnityEditor;

// Tools -> Reset Story Progress: стирает сохранённые флаги диалогов
// (и все PlayerPrefs игры), чтобы протестировать игру с самого начала.
public static class ResetStoryProgress
{
    [MenuItem("Tools/Reset Story Progress")]
    private static void Reset()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[ResetStoryProgress] Все сохранённые флаги сброшены — игра начнётся с начала.");
    }
}
