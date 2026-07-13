using UnityEngine;

public class HeroMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float sprintMultiplier = 2f; // ускорение при зажатом Shift

    private SpriteRenderer spriteRenderer;
    private Camera cam;
    private CharacterController cc;
    private float lockedY; // высота, на которой герой стоит всегда

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        cam = Camera.main;
        cc = GetComponent<CharacterController>();
        lockedY = transform.position.y;
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 camForward = cam.transform.forward;
        Vector3 camRight   = cam.transform.right;
        camForward.y = 0f; camForward.Normalize();
        camRight.y   = 0f; camRight.Normalize();

        Vector3 direction = (camRight * h + camForward * v).normalized;

        // Спринт: зажатый Shift ускоряет героя
        float currentSpeed = speed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            currentSpeed *= sprintMultiplier;

        cc.Move(direction * currentSpeed * Time.deltaTime);

        // Держим высоту: CharacterController без гравитации может «заступить»
        // на чужой коллайдер (зверёк, стык земли) и остаться наверху
        if (!Mathf.Approximately(transform.position.y, lockedY))
        {
            Vector3 p = transform.position;
            p.y = lockedY;
            transform.position = p;
            Physics.SyncTransforms();
        }

        spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.z * 10) + 100;
    }
}
