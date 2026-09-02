using UnityEngine;

// Язык игры. Сейчас игра полностью на польском:
// тексты диалогов лежат в ассетах, надписи интерфейса — прямо в сценах.
// Класс оставлен, чтобы кнопка языка в настройках продолжала работать.
public static class Loc
{
    public const string Key = "language";
    public static readonly string[] Names = { "Polski" };

    public static event System.Action OnLanguageChanged;

    public static int Index
    {
        get => 0;
        set { if (value != 0) OnLanguageChanged?.Invoke(); }
    }

    public static string CurrentName => Names[0];
    public static bool IsPolish => true;

    public static void Next() { /* язык один — переключать нечего */ }

    // Текст как есть: перевод уже вшит в сцены и ассеты
    public static string Pick(string en, string pl) => string.IsNullOrEmpty(pl) ? en : pl;
    public static string UI(string text) => text;
}
