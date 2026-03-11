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

        float explosiveFactor = explosiveKg / explosiveMin;
        float wallFactor = thicknessMin / thicknessCm;

        float effectiveness = explosiveFactor * wallFactor;
        effectiveness = Mathf.Clamp(effectiveness, 0.3f, 6f);

        // hole size
        float holeDiameter = 40f * Mathf.Pow(effectiveness, 0.5f);

        // blast radius
        float blastRadius = holeDiameter * 0.6f;

        // explosion force
        float explosionForce = 1000f * effectiveness;

        // fragments
        int iterations = Mathf.Clamp(
            Mathf.RoundToInt(2 + effectiveness * 1.5f),
            2,
            8
        );

        // normalize hole
        float normalizedHole = effectiveness / 6f;
        normalizedHole = Mathf.Clamp01(normalizedHole);

        Debug.Log(
        $"E:{explosiveKg}kg  T:{thicknessCm}cm  Eff:{effectiveness}  Hole:{holeDiameter}  Norm:{normalizedHole}"
        );

        wallShatter.SetSettingsValues(
            normalizedHole,
            iterations,
            explosionForce,
            blastRadius * 0.01f
        );

        ExplosionChange?.Invoke(Mathf.RoundToInt(explosiveKg * 1000));
    }

    public void OnResetPressed()
    {
        wallShatter.ResetWall();
    }
}