using System.Collections;
using UnityEngine;

// Цветок, оживающий, когда герой пробегает рядом (Q4).
// Повесить на цветок со SpriteRenderer + Collider (Is Trigger!).
// При входе героя цветок «подпрыгивает» (squash & stretch) и,
// если задано, играет частицы. Работает многократно.
public class FlowerPop : MonoBehaviour
{
    [Header("Анимация")]
    public float popScale = 1.25f;   // насколько «раздувается»
    public float popTime = 0.35f;    // длительность отскока
    public ParticleSystem popParticles; // необязательно: пыльца

    [Header("Повторное срабатывание")]
    public float cooldown = 1f;

    private Vector3 baseScale;
    private bool animating;
    private float lastPop = -99f;

    void Start()
    {
        baseScale = transform.localScale;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Hero")) return;
        if (animating || Time.time - lastPop < cooldown) return;
        StartCoroutine(Pop());
    }

    private IEnumerator Pop()
    {
        animating = true;
        lastPop = Time.time;
        if (popParticles != null) popParticles.Play();

        float half = popTime * 0.5f;
        // раздуться
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            float k = Mathf.Sin((t / half) * Mathf.PI * 0.5f); // ease-out
            transform.localScale = Vector3.Lerp(baseScale, baseScale * popScale, k);
            yield return null;
        }
        // вернуться с лёгким перелётом
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            float k = t / half;
            float bounce = 1f + (popScale - 1f) * (1f - k) * Mathf.Cos(k * Mathf.PI * 2f) * 0.5f;
            transform.localScale = baseScale * Mathf.Max(bounce, 0.9f);
            yield return null;
        }
        transform.localScale = baseScale;
        animating = false;
    }
}
