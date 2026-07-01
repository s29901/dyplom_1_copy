using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BubbleController : MonoBehaviour
{
    [Header("Настройки")]
    public string phrase;           // tekst frazy ("boję się" itp.)
    public Color silentColor = Color.gray;
    public Color spokenColor = Color.yellow;

    [Header("UI")]
    public GameObject buttonPanel;  // panel z przyciskami

    private SpriteRenderer sr;
    private bool isSaid = false;
    private Vector3 startPos;
    private float floatSpeed = 1f;
    private float floatAmount = 0.3f;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = silentColor;
        startPos = transform.position;
        if (buttonPanel != null)
            buttonPanel.SetActive(false);
    }

    void Update()
    {
        if (!isSaid)
        {
            // kołysanie w górę i w dół
            float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
            transform.position = new Vector3(startPos.x, newY, startPos.z);
        }
    }

    void OnMouseDown()
    {
        if (!isSaid && buttonPanel != null)
            buttonPanel.SetActive(true);
    }

    public void OnSay()
    {
        isSaid = true;
        sr.color = spokenColor;
        buttonPanel.SetActive(false);
        // wzlatuje do góry
        StartCoroutine(FlyUp());
        // informujemy menedżera
        FindObjectOfType<QuestManager_Q3>().OnBubbleSaid();
    }

    public void OnSilent()
    {
        buttonPanel.SetActive(false);
    }

    System.Collections.IEnumerator FlyUp()
    {
        float t = 0f;
        Vector3 start = transform.position;
        Vector3 end = start + new Vector3(0, 5f, 0);
        while (t < 1f)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, t);
            sr.color = Color.Lerp(spokenColor, new Color(1,1,1,0), t);
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
