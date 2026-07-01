using UnityEngine;
using UnityEngine.SceneManagement; // potrzebne do pracy ze scenami

public class SceneLoader : MonoBehaviour
{
    // Wczytuje każdą scenę po nazwie, używane wszędzie
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Szybki powrót do hubu, będziemy wywoływać po każdym queście
    public void LoadHub()
    {
        SceneManager.LoadScene("02_HubGarden");
    }

    // Następna scena w kolejności, przydatne dla intro i zakończenia
    public void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentIndex + 1);
    }
}