using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    // Singleton, jeden na całą grę, dostępny z każdej sceny
    public static ProgressManager Instance;

    // Które questy zostały zakończone
    public bool quest1Done = false;
    public bool quest2Done = false;
    public bool quest3Done = false;
    public bool quest4Done = false;

    // Ile questów zakończono (0-4), określa etap rośliny
    public int QuestsCompleted()
    {
        int count = 0;
        if (quest1Done) count++;
        if (quest2Done) count++;
        if (quest3Done) count++;
        if (quest4Done) count++;
        return count;
    }

    void Awake()
    {
        // Jeśli menedżer już istnieje, usuwamy duplikat
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        // Zapamiętujemy siebie i nie jesteśmy usuwani przy zmianie scen
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}