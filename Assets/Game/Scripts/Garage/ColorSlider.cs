using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ColorSlider : MonoBehaviour
{
    public Color color; // Main color.

    // Sliders per color channel.
    public Slider redSlider;
    public Slider greenSlider;
    public Slider blueSlider;

    [Header("Car body")] public Slider metalnessSlider;
    public Slider glossinessSlider;

    public float metalness;
    public float glossiness;

    public void Update()
    {
        // Assigning new color to main color.
        color = new Color(redSlider.value, greenSlider.value, blueSlider.value);

        if (metalnessSlider)
        {
            metalness = metalnessSlider.value;
        }

        if (glossinessSlider)
        {
            glossiness = glossinessSlider.value;
        }
    }
}
