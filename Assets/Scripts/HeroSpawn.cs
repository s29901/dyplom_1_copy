using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Повесить на Hero в каждой сцене.
// Запоминает, где герой стоял в каждой сцене (позицию сохраняет портал
// в момент входа), и при возвращении в сцену ставит героя на то же место.
// Работает в рамках одной игровой сессии.
public class HeroSpawn : MonoBehaviour
{
    private static readonly Dictionary<string, Vector3> savedPositions =
        new Dictionary<string, Vector3>();

    // Вызывается порталом при входе героя
    public static void SavePosition(Transform hero)
    {
        savedPositions[SceneManager.GetActiveScene().name] = hero.position;
    }

    private void Start()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (!savedPositions.TryGetValue(scene, out Vector3 pos)) return;

        // CharacterController надо выключить на время телепорта,
        // иначе он вернёт героя обратно
        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        transform.position = pos;
        if (cc != null) cc.enabled = true;
    }
}
