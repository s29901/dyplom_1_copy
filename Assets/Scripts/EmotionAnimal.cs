using System.Collections;
using UnityEngine;

// Зверёк-чувство для Q3 (грусть, злость, усталость, страх, обида).
// Медленно бродит по поляне. По клику рассказывает о своём чувстве
// (диалог с выбором). Когда его выслушали — меняет спрайт на спокойный,
// играет частицы и (если задана точка сбора) идёт к дереву.
// Требуется Collider на этом объекте для клика.
public class EmotionAnimal : MonoBehaviour
{
    [Header("Диалог этого зверька")]
    public DialogueData dialogue;

    [Header("Спрайты: грустный / спокойный")]
    public Sprite sadSprite;
    public Sprite calmSprite;

    [Header("Зона прогулки (мировые X/Z)")]
    public float minX = -10f;
    public float maxX = 10f;
    public float minZ = -15f;
    public float maxZ = 5f;
    public float moveSpeed = 1.2f;
    public float pauseTime = 2f;        // передышка между переходами
    public bool spriteFacesLeft = true; // куда «смотрит» исходный спрайт

    [Header("После утешения")]
    public Transform gatherPoint;        // точка сбора у дерева (можно пусто)
    public ParticleSystem comfortParticles;

    public bool Heard { get; private set; }

    private SpriteRenderer sr;
    private Vector3 target;
    private float pause;
    private bool talking;
    private bool atGatherPoint;

    void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null && sadSprite != null) sr.sprite = sadSprite;
        PickNewTarget();
    }

    void Update()
    {
        HandleClick();

        if (talking || atGatherPoint) return;

        // Пока зверька не выслушали — он сидит неподвижно со своим чувством.
        if (!Heard) return;

        // Диалог на экране (любой) — зверьки замирают
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
            return;

        // Выслушанный зверёк оживает: идёт к точке сбора или спокойно гуляет
        if (gatherPoint != null)
        {
            if (StepTowards(gatherPoint.position))
                atGatherPoint = true;
            return;
        }

        if (pause > 0f) { pause -= Time.deltaTime; return; }

        if (StepTowards(target))
        {
            pause = pauseTime + Random.Range(0f, 1.5f);
            PickNewTarget();
        }
    }

    // Шаг к цели; true — пришли
    private bool StepTowards(Vector3 to)
    {
        to.y = transform.position.y;
        FaceTowards(to);
        transform.position = Vector3.MoveTowards(transform.position, to, moveSpeed * Time.deltaTime);
        return Vector3.Distance(transform.position, to) < 0.1f;
    }

    private void FaceTowards(Vector3 to)
    {
        float dx = to.x - transform.position.x;
        if (Mathf.Abs(dx) < 0.01f) return;
        bool movingLeft = dx < 0f;
        var s = transform.localScale;
        s.x = Mathf.Abs(s.x) * ((movingLeft == spriteFacesLeft) ? 1f : -1f);
        transform.localScale = s;
    }

    private void PickNewTarget()
    {
        target = new Vector3(Random.Range(minX, maxX), transform.position.y, Random.Range(minZ, maxZ));
    }

    // Клик через RaycastAll: срабатывает, даже если поверх зверька
    // лежит другой коллайдер (земля, CameraConfiner и т.п.)
    private void HandleClick()
    {
        if (Heard || talking || dialogue == null) return;
        if (!Input.GetMouseButtonDown(0)) return;
        if (DialogueManager.Instance == null || DialogueManager.Instance.IsDialogueActive) return;

        var cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        foreach (var hit in Physics.RaycastAll(ray, 1000f))
        {
            if (hit.collider != null && hit.collider.enabled &&
                hit.collider.transform.IsChildOf(transform))
            {
                StartCoroutine(Talk());
                return;
            }
        }
    }

    private IEnumerator Talk()
    {
        talking = true;
        DialogueManager.Instance.StartDialogue(dialogue);
        yield return null;
        while (DialogueManager.Instance.IsDialogueActive)
            yield return null;

        talking = false;
        Heard = true;

        if (sr != null && calmSprite != null) sr.sprite = calmSprite;
        if (comfortParticles != null) comfortParticles.Play();

        var qm = FindFirstObjectByType<QuestManager_Q3>();
        if (qm != null) qm.OnAnimalHeard();
    }
}
