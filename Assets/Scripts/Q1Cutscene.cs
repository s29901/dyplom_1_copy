using System.Collections;
using UnityEngine;

// Вступительная катсцена Q1:
// 1) камера наезжает на солнце и возвращается;
// 2) герой сам подходит к птице;
// 3) первая часть диалога;
// 4) камера наезжает на росток, вторая часть диалога;
// 5) камера возвращается, игрок получает управление и может взять солнце.
// Повесить на пустой объект в сцене Q1 (например, "Q1Cutscene").
public class Q1Cutscene : MonoBehaviour
{
    [Header("Диалоги")]
    public DialogueData introPart1; // до "Sometimes light alone isn't enough."
    public DialogueData introPart2; // от "My tree looks so small here."

    [Header("Герой")]
    public HeroMovement heroMovement;
    public Transform hero;
    public Transform birdPoint;   // пустышка возле птицы — куда герой подходит
    public float walkSpeed = 3f;

    [Header("Солнце")]
    public Collider sunCollider;  // коллайдер солнца — заблокирован до конца диалога

    [Header("Камеры (Cinemachine, выключенные)")]
    public GameObject sunCamera;   // камера, смотрящая на солнце
    public GameObject treeCamera;  // камера, смотрящая на росток
    public float sunLookTime = 3f;   // сколько секунд смотрим на солнце
    public float blendTime = 1.5f;   // время возврата камеры (= Default Blend у Brain)

    [Header("UI квеста")]
    public GameObject warmthBar; // WarmthBarBackground — появится после диалога

    [Header("Прочее")]
    public bool onlyOnce = true;  // не повторять катсцену при повторном входе в сцену

    private static bool played; // в рамках одной игровой сессии

    // Закончилось ли интро этой сцены (для клика по птице и т.п.)
    public static bool IntroFinished { get; private set; }

    private void Awake()
    {
        IntroFinished = false;
    }

    private IEnumerator Start()
    {
        if (played && onlyOnce)
        {
            IntroFinished = true; // интро уже показывали ранее
            yield break;
        }
        played = true;

        if (heroMovement != null) heroMovement.enabled = false;
        if (sunCollider != null) sunCollider.enabled = false;
        if (warmthBar != null) warmthBar.SetActive(false); // бар спрятан до конца диалога

        yield return null; // кадр на инициализацию камер и менеджеров

        // 1. Наезд на солнце и возврат
        if (sunCamera != null)
        {
            sunCamera.SetActive(true);
            yield return new WaitForSeconds(blendTime + sunLookTime);
            sunCamera.SetActive(false);
            yield return new WaitForSeconds(blendTime);
        }

        // 2. Герой подходит к птице
        if (hero != null && birdPoint != null)
        {
            Vector3 target = new Vector3(birdPoint.position.x, hero.position.y, birdPoint.position.z);
            while (Vector3.Distance(hero.position, target) > 0.05f)
            {
                hero.position = Vector3.MoveTowards(hero.position, target, walkSpeed * Time.deltaTime);
                yield return null;
            }
        }

        // 3. Первая часть диалога
        yield return PlayDialogue(introPart1);

        // 4. Наезд на росток и вторая часть диалога
        if (treeCamera != null)
        {
            treeCamera.SetActive(true);
            yield return new WaitForSeconds(blendTime);
        }

        yield return PlayDialogue(introPart2);

        if (treeCamera != null)
        {
            treeCamera.SetActive(false);
            yield return new WaitForSeconds(blendTime);
        }

        // 5. Показываем шкалу тепла и отдаём управление
        if (warmthBar != null) warmthBar.SetActive(true);
        if (heroMovement != null) heroMovement.enabled = true;
        if (sunCollider != null) sunCollider.enabled = true;
        IntroFinished = true;
    }

    private IEnumerator PlayDialogue(DialogueData dialogue)
    {
        if (dialogue == null || DialogueManager.Instance == null) yield break;

        DialogueManager.Instance.StartDialogue(dialogue);
        while (DialogueManager.Instance.IsDialogueActive)
            yield return null;
    }
}
