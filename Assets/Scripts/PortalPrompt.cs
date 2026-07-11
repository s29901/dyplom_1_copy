using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Окно подтверждения перехода через портал.
// Повесить на объект в Canvas (например, панель PortalPromptPanel).
// В Inspector назначить: panel (сама панель), questionText, yesButton, noButton.
public class PortalPrompt : MonoBehaviour
{
    public static PortalPrompt Instance { get; private set; }

    [SerializeField] private GameObject panel;      // корневая панель окна
    [SerializeField] private TMP_Text questionText; // текст вопроса
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private string targetScene;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        yesButton.onClick.AddListener(OnYes);
        noButton.onClick.AddListener(Hide);
        panel.SetActive(false);
    }

    public void Show(string sceneName, string question)
    {
        targetScene = sceneName;
        questionText.text = question;
        yesButton.gameObject.SetActive(true);
        noButton.gameObject.SetActive(true);
        panel.SetActive(true);
    }

    // Сообщение без кнопок — для закрытых порталов
    public void ShowLocked(string message)
    {
        targetScene = null;
        questionText.text = message;
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
        targetScene = null;
    }

    private void OnYes()
    {
        if (!string.IsNullOrEmpty(targetScene))
            SceneManager.LoadScene(targetScene);
    }
}
