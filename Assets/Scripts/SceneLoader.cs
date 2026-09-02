using UnityEngine;
using UnityEngine.SceneManagement; // potrzebne do pracy ze scenami

public class SceneLoader : MonoBehaviour
{
    // Wczytuje każdą scenę po nazwie, używane wszędzie
    public void LoadScene(string sceneName)
    {
        SceneTransition.Load(sceneName);
    }

    // Szybki powrót do hubu, będziemy wywoływać po każdym queście
    public void LoadHub()
    {
        SceneTransition.Load("02_HubGarden");
    }

    // Następna scena w kolejności, przydatne dla intro i zakończenia
    public void LoadNextScene()
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        string next = System.IO.Path.GetFileNameWithoutExtension(
            SceneUtility.GetScenePathByBuildIndex(nextIndex));
        SceneTransition.Load(next);
    }
}