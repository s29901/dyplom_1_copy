using UnityEngine;
using UnityEngine.UI;

public class WarmthBar : MonoBehaviour
{
    // Referencja do wypełnienia, przeciągniemy w Inspectorze
    [SerializeField] private Image fillImage;

    // Ustawia wypełnienie od 0 do 1
    public void SetFill(float value)
    {
        fillImage.fillAmount = Mathf.Clamp01(value);
    }
}
