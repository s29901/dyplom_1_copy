using UnityEngine;

public class HeroMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private SpriteRenderer spriteRenderer;
    private Camera cam;
    private CharacterController cc;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        cam = Camera.main;
        cc = GetComponent<CharacterController>();
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

        cc.Move(direction * speed * Time.deltaTime);

        spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.z * 10) + 100;
    }
}
