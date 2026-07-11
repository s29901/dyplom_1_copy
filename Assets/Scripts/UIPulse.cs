using UnityEngine;

// Лёгкая пульсация масштаба. Повесить на кнопку или любой UI-элемент.
public class UIPulse : MonoBehaviour
{
    [SerializeField] private float speed = 2.5f;   // скорость пульсации
    [SerializeField] private float amount = 0.06f; // сила (0.06 = ±6% размера)

    private Vector3 baseScale;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    private void OnDisable()
    {
        transform.localScale = baseScale; // возвращаем исходный размер
    }

    private void Update()
    {
        float pulse = 1f + Mathf.Sin(Time.time * speed) * amount;
        transform.localScale = baseScale * pulse;
    }
}
