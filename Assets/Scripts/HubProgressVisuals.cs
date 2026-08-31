using System.Collections;
using UnityEngine;

// Преображение сада (хаба) по прогрессу.
// Повесить на объект "garden" (или указать группы вручную).
// Структура: garden > before_quests, after_q1, after_q2, after_q3, after_q4.
//
// Режим Replace: видна только группа текущей стадии (сад меняется целиком).
// Режим Accumulate: видны все группы до текущей включительно (сад дополняется).
public class HubProgressVisuals : MonoBehaviour
{
    public enum Mode { Replace, Accumulate }

    [Header("Как работают группы")]
    public Mode mode = Mode.Replace;

    [Header("Группы (пусто = найдутся по именам среди детей)")]
    public GameObject beforeQuests;  // before_quests
    public GameObject afterQ1;       // after_q1
    public GameObject afterQ2;       // after_q2
    public GameObject afterQ3;       // after_q3
    public GameObject afterQ4;       // after_q4

    [Header("Плавное появление новой стадии")]
    public bool fadeIn = true;
    public float fadeDuration = 1.2f;

    private GameObject[] groups;

    void Start()
    {
        AutoFindGroups();

        int done = ProgressManager.Instance != null
            ? ProgressManager.Instance.QuestsCompleted() : 0;
        done = Mathf.Clamp(done, 0, groups.Length - 1);

        // Стадия 0 = before_quests, стадия 1 = after_q1 и т.д.
        for (int i = 0; i < groups.Length; i++)
        {
            if (groups[i] == null) continue;
            bool visible = mode == Mode.Accumulate ? i <= done : i == done;
            groups[i].SetActive(visible);
        }

        // Новую стадию мягко проявляем (кроме самого первого входа в игру)
        if (fadeIn && done > 0 && groups[done] != null)
            StartCoroutine(FadeInGroup(groups[done]));
    }

    private void AutoFindGroups()
    {
        if (beforeQuests == null) beforeQuests = FindChild("before_quests");
        if (afterQ1 == null) afterQ1 = FindChild("after_q1");
        if (afterQ2 == null) afterQ2 = FindChild("after_q2");
        if (afterQ3 == null) afterQ3 = FindChild("after_q3");
        if (afterQ4 == null) afterQ4 = FindChild("after_q4");

        groups = new[] { beforeQuests, afterQ1, afterQ2, afterQ3, afterQ4 };
    }

    // Ищет дочерний объект по имени, в том числе выключенный
    private GameObject FindChild(string name)
    {
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
            if (t.name.Trim() == name) return t.gameObject;
        return null;
    }

    private IEnumerator FadeInGroup(GameObject group)
    {
        var renderers = group.GetComponentsInChildren<SpriteRenderer>(true);
        if (renderers.Length == 0) yield break;

        var targetAlpha = new float[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            targetAlpha[i] = renderers[i].color.a;
            var c = renderers[i].color; c.a = 0f;
            renderers[i].color = c;
        }

        for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
        {
            float k = t / fadeDuration;
            for (int i = 0; i < renderers.Length; i++)
            {
                var c = renderers[i].color;
                c.a = Mathf.Lerp(0f, targetAlpha[i], k);
                renderers[i].color = c;
            }
            yield return null;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            var c = renderers[i].color;
            c.a = targetAlpha[i];
            renderers[i].color = c;
        }
    }
}
