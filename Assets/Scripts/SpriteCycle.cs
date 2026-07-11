using UnityEngine;

// Зацикленная смена спрайтов (например, анимация фона из 3 кадров).
// Повесить на объект со SpriteRenderer, в frames перетащить спрайты по порядку.
public class SpriteCycle : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;          // кадры по порядку
    [SerializeField] private float frameDuration = 0.5f; // сколько секунд держится один кадр

    private SpriteRenderer spriteRenderer;
    private float timer;
    private int index;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (frames != null && frames.Length > 0 && frames[0] != null)
            spriteRenderer.sprite = frames[0];
    }

    private void Update()
    {
        if (frames == null || frames.Length < 2) return;

        timer += Time.deltaTime;
        if (timer >= frameDuration)
        {
            timer -= frameDuration;
            index = (index + 1) % frames.Length;
            if (frames[index] != null)
                spriteRenderer.sprite = frames[index];
        }
    }
}
