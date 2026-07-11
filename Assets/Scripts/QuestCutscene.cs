using System.Collections;
using UnityEngine;

// Универсальная вступительная катсцена квестовой сцены:
// 1) герой сам подходит к птице;
// 2) первая часть диалога;
// 3) камера наезжает на растение, вторая часть диалога;
// 4) камера возвращается, включается UI квеста и интерактив, игрок получает управление.
// Повесить на пустой объект (например, "Q2Cutscene").
public class QuestCutscene : MonoBehaviour
{
    [Header("Ключ (уникальный для сцены)")]
    public string cutsceneId = "q2_intro";
    public bool oncePerGame = true; // хранится в PlayerPrefs, сброс через Tools -> Reset Story Progress

    [Header("Диалоги")]
    public DialogueData introPart1; // до наезда камеры
    public DialogueData introPart2; // во время наезда на растение

    [Header("Герой")]
    public HeroMovement heroMovement;
    public Transform hero;
    public Transform birdPoint;   // пустышка возле птицы
    public float walkSpeed = 3f;

    [Header("Камера (Cinemachine, выключенная)")]
    public GameObject focusCamera; // камера, смотрящая на растение
    public float blendTime = 1.5f;

    [Header("Квест: включить после диалога")]
    public GameObject questUI;        // шкала/индикатор (может быть пустым)
    public Collider[] interactables;  // коллайдеры облаков/солнца — заблокированы до конца интро

    public bool IntroFinished { get; private set; }

    private IEnumerator Start()
    {
        if (oncePerGame && PlayerPrefs.GetInt("cutscene_" + cutsceneId, 0) == 1)
        {
            IntroFinished = true; // интро уже показывали — сразу обычный режим
            yield break;
        }

        if (heroMovement != null) heroMovement.enabled = false;
        if (questUI != null) questUI.SetActive(false);
        SetInteractables(false);

        yield return null; // кадр на инициализацию камер и менеджеров

        // Герой подходит к птице
        if (hero != null && birdPoint != null)
        {
            Vector3 target = new Vector3(birdPoint.position.x, hero.position.y, birdPoint.position.z);
            while (Vector3.Distance(hero.position, target) > 0.05f)
            {
                hero.position = Vector3.MoveTowards(hero.position, target, walkSpeed * Time.deltaTime);
                yield return null;
            }
        }

        yield return PlayDialogue(introPart1);

        if (focusCamera != null)
        {
            focusCamera.SetActive(true);
            yield return new WaitForSeconds(blendTime);
        }

        yield return PlayDialogue(introPart2);

        if (focusCamera != null)
        {
            focusCamera.SetActive(false);
            yield return new WaitForSeconds(blendTime);
        }

        if (questUI != null) questUI.SetActive(true);
        SetInteractables(true);
        if (heroMovement != null) heroMovement.enabled = true;
        IntroFinished = true;

        PlayerPrefs.SetInt("cutscene_" + cutsceneId, 1);
        PlayerPrefs.Save();
    }

    private void SetInteractables(bool enabled)
    {
        if (interactables == null) return;
        foreach (var c in interactables)
            if (c != null) c.enabled = enabled;
    }

    private IEnumerator PlayDialogue(DialogueData dialogue)
    {
        if (dialogue == null || DialogueManager.Instance == null) yield break;

        DialogueManager.Instance.StartDialogue(dialogue);
        while (DialogueManager.Instance.IsDialogueActive)
            yield return null;
    }
}
