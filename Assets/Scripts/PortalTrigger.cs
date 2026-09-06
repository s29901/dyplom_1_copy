using UnityEngine;

public class PortalTrigger : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [TextArea]
    [SerializeField] private string question = "Перейти в другую локацию?";

    [Header("Порядок открытия")]
    // Какой квест должен быть пройден, чтобы портал открылся:
    // 0 = портал всегда открыт, 1 = нужен квест 1, 2 = квест 2 и т.д.
    [SerializeField] private int requiredQuest = 0;
    [TextArea]
    [SerializeField] private string lockedMessage =
        "Наверное, лучше сначала разобраться с другим местом...";

    [Header("Требуется разговор с птицей")]
    [Tooltip("Портал закрыт, пока не состоялся вступительный разговор")]
    [SerializeField] private bool requireBirdTalk = true;

    [Tooltip("Тот же ключ, что в Dialogue Id у BirdInteraction")]
    [SerializeField] private string birdDialogueId = "hub_bird_intro";

    [TextArea]
    [SerializeField] private string birdLockedMessage = "Porozmawiaj z ptakiem";

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Hero") || PortalPrompt.Instance == null) return;

        // Запоминаем, где герой стоит: при возвращении в эту сцену
        // он появится здесь же, у портала
        HeroSpawn.SavePosition(other.transform);

        // Сначала вступительный разговор, только потом дорога дальше
        if (!BirdTalked())
        {
            PortalPrompt.Instance.ShowLocked(birdLockedMessage);
            return;
        }

        if (IsUnlocked())
            PortalPrompt.Instance.Show(sceneName, question);
        else
            PortalPrompt.Instance.ShowLocked(lockedMessage);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hero") && PortalPrompt.Instance != null)
        {
            PortalPrompt.Instance.Hide();
        }
    }

    // Разговор помечается пройденным уже при открытии панели,
    // поэтому пока диалог на экране портал всё ещё считается закрытым
    private bool BirdTalked()
    {
        if (!requireBirdTalk) return true;
        if (!BirdInteraction.WasPlayed(birdDialogueId)) return false;

        var dm = DialogueManager.Instance;
        return dm == null || !dm.IsDialogueActive;
    }

    private bool IsUnlocked()
    {
        if (requiredQuest <= 0) return true;

        var pm = ProgressManager.Instance;
        if (pm == null) return true; // нет менеджера (тест сцены напрямую) — не блокируем

        switch (requiredQuest)
        {
            case 1: return pm.quest1Done;
            case 2: return pm.quest2Done;
            case 3: return pm.quest3Done;
            case 4: return pm.quest4Done;
            default: return true;
        }
    }
}
