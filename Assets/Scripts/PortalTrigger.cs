using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalTrigger : MonoBehaviour
{
    [SerializeField] private string sceneName;

    void OnTriggerEnter(Collider other)
    {
        // Wyświetlamy w konsoli, CO weszło do triggera
        Debug.Log("Триггер сработал! Объект: " + other.gameObject.name + " Тег: " + other.tag);
        
        if (other.CompareTag("Hero"))
        {
            Debug.Log("Загружаем сцену: " + sceneName);
            SceneManager.LoadScene(sceneName);
        }
    }
}