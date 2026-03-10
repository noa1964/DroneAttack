using System;
using UnityEngine;

public class ShatterUIController : MonoBehaviour
{
    [SerializeField] private WallShatter wallShatter;

    [Header("Sliders")]
    [SerializeField] private SliderWithLabel _ExplosionSlider;   // 0-10
    [SerializeField] private SliderWithLabel _ThicknessSlider;   // 0-10

    [Header("Real Ranges")]
    [SerializeField] private float explosiveMin = 0.2f; // kg (200g)
    [SerializeField] private float explosiveMax = 2f;   // kg (2kg)

    [SerializeField] private float thicknessMin = 5f;   // cm
    [SerializeField] private float thicknessMax = 40f;  // cm

    public static Action<int> ExplosionChange;

    private void Start()
    {
        _ExplosionSlider.OnValueChanged += UpdateShatterValues;
        _ThicknessSlider.OnValueChanged += UpdateShatterValues;

        UpdateShatterValues(0);
    }

    private void UpdateShatterValues(float value)
    {
        float explosionSlider = _ExplosionSlider.GetCurrentValue() / 10f;
        float thicknessSlider = _ThicknessSlider.GetCurrentValue() / 10f;

        float explosiveKg = Mathf.Lerp(explosiveMin, explosiveMax, explosionSlider);
        float thicknessCm = Mathf.Lerp(thicknessMin, thicknessMax, thicknessSlider);

        // ---- PHYSICS EQUATION ----

        float holeDiameter =
            40f *
            Mathf.Pow(explosiveKg / 0.3f, 1f / 3f) *
            (20f / thicknessCm);

        float blastRadius = holeDiameter * 0.5f;

        // normalize hole size for the wall script
        float normalizedHole = Mathf.Clamp01(
    Mathf.InverseLerp(20f, 120f, holeDiameter)
);

        // iterations (more explosive + thinner wall)
        int iterations = Mathf.Clamp(
            Mathf.RoundToInt(
                2 + 4 *
                Mathf.Pow(explosiveKg / 0.3f, 1f / 3f) *
                (20f / thicknessCm)
            ),
            2,
            8
        );

        float explosionForce = 1200f * explosiveKg;

        Debug.Log(
    $"Explosive {explosiveKg}kg | Thickness {thicknessCm}cm | Hole {holeDiameter}cm | Norm {normalizedHole}"
);

        wallShatter.SetSettingsValues(
            normalizedHole,
            iterations,
            explosionForce,
            blastRadius / 100f
        );

        ExplosionChange?.Invoke(Mathf.RoundToInt(explosiveKg * 1000));
    }

    public void OnResetPressed()
    {
        wallShatter.ResetWall();
    }
}