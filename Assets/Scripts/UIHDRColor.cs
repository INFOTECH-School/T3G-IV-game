using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class UIHDRColor : MonoBehaviour
{
    // The second 'true' enables the HDR color picker in the Inspector
    [ColorUsage(true, true)] 
    public Color myGlowColor = Color.white;

    void Start()
    {
        ApplyHDR();
    }

    void OnValidate() // Updates the color in the editor when you change it
    {
        ApplyHDR();
    }

    private void ApplyHDR()
    {
        Graphic uiElement = GetComponent<Graphic>();
        if (uiElement != null)
        {
            uiElement.color = myGlowColor;
        }
    }
}