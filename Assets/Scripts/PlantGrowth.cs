using UnityEngine;

public class PlantGrowth : MonoBehaviour
{
    // Tutaj przeciągniesz 5 sprite'ów w Inspectorze, od ziarenka do drzewa
    [SerializeField] private Sprite[] growthStages = new Sprite[5];

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdatePlant(); // pokazujemy odpowiedni etap na starcie
    }

    // Wywoływane na starcie i po każdym queście
    public void UpdatePlant()
    {
        if (ProgressManager.Instance == null) return;

        // Sprawdzamy, ile questów zostało zakończonych (0-4)
        int stage = ProgressManager.Instance.QuestsCompleted();

        // Zabezpieczenie, nie wychodzimy poza zakres tablicy
        stage = Mathf.Clamp(stage, 0, growthStages.Length - 1);

        // Zmieniamy sprite, jeśli jest przypisany
        if (growthStages[stage] != null)
            spriteRenderer.sprite = growthStages[stage];
    }
}