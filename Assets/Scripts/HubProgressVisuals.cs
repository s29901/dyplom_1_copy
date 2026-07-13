using UnityEngine;

// Преображение сада (хаба) по прогрессу.
// Повесить на пустой объект в хабе. В каждый список перетащить объекты
// (цветы, трава, краски, огоньки...), которые должны ПОЯВИТЬСЯ после
// соответствующего квеста. Списки накапливаются: после квеста 2 включено
// всё из списков 1 и 2 и т.д.
public class HubProgressVisuals : MonoBehaviour
{
    [Header("Появляется после квеста 1 (тепло)")]
    public GameObject[] afterQuest1;
    [Header("Появляется после квеста 2 (слёзы)")]
    public GameObject[] afterQuest2;
    [Header("Появляется после квеста 3 (чувства)")]
    public GameObject[] afterQuest3;
    [Header("Появляется после квеста 4 (отдых) — финальный сад")]
    public GameObject[] afterQuest4;

    [Header("Скрыть, когда сад полностью расцвёл (руины, трещины...)")]
    public GameObject[] hideWhenComplete;

    void Start()
    {
        int done = ProgressManager.Instance != null
            ? ProgressManager.Instance.QuestsCompleted() : 0;

        SetAll(afterQuest1, done >= 1);
        SetAll(afterQuest2, done >= 2);
        SetAll(afterQuest3, done >= 3);
        SetAll(afterQuest4, done >= 4);
        SetAll(hideWhenComplete, done < 4);
    }

    private static void SetAll(GameObject[] arr, bool on)
    {
        if (arr == null) return;
        foreach (var go in arr)
            if (go != null && go.activeSelf != on) go.SetActive(on);
    }
}
