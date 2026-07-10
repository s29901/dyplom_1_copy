using UnityEngine;

public class PortalTrigger : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [TextArea]
    [SerializeField] private string question = "Перейти в другую локацию?";

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hero") && PortalPrompt.Instance != null)
        {
            PortalPrompt.Instance.Show(sceneName, question);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hero") && PortalPrompt.Instance != null)
        {
            PortalPrompt.Instance.Hide();
        }
    }
}
