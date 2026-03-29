using System;
using UnityEngine;

public class ShatterUIController : MonoBehaviour
{
    [SerializeField] private WallShatter wallShatter;

    [Header("Sliders")]
    [SerializeField] private SliderWithLabel _ExplosionSlider;   // 0-10
    [SerializeField] private SliderWithLabel _ThicknessSlider;   // 0-10

    [Header("Real Ranges")]
    [SerializeField] private float explosiveMin = 200f; // kg (200g)
    [SerializeField] private float explosiveMax = 2000f;   // kg (2kg)

    [SerializeField] private float thicknessMin = 5f;   // cm
    [SerializeField] private float thicknessMax = 40f;  // cm

    public static Action<int, float> ExplosionChange;

    private void Start()
    {
        _ExplosionSlider.OnValueChanged += UpdateShatterValues;
        _ThicknessSlider.OnValueChanged += UpdateShatterValues;

        UpdateShatterValues(0);
    }

    float Map(float value, float inMin, float inMax, float outMin, float outMax)
    {
        return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
    }

    private void UpdateShatterValues(float value)
    {
        float explosiveGrams = /*Map(*/_ExplosionSlider.GetCurrentValue()/*, 1f, 10f, explosiveMin, explosiveMax)*/;
        float wallThicknessCm = /*Map(*/_ThicknessSlider.GetCurrentValue()/*, 1f, 10f, thicknessMin, thicknessMax)*/;

        float holeDiameter = CalculateHoleDiameter(explosiveGrams, wallThicknessCm);
        float blastBehindWall = CalculateBlastBehindWall(explosiveGrams, wallThicknessCm);

        float normalizedHole = Mathf.InverseLerp(27f, 62f, holeDiameter);

        // --- RADIUS CALCULATION START ---
        
        // 1. Get the raw intensity from your existing exponential function (0 to 1)
        float intensity = blastBehindWall; 

        // 2. Apply a Power function to make it non-linear. 
        // A value of 0.75 makes the radius grow quickly at first, then slow down.
        // A value of 1.5 would make it stay small longer and then "explode" at the end.
        float nonLinearFactor = Mathf.Pow(intensity, 0.8f); 

        // 3. Map that factor to your 1-9 range
        float blastRadius = Mathf.Lerp(1f, 9f, nonLinearFactor);

        // --- RADIUS CALCULATION END ---

        int iterations = Mathf.Clamp(
            Mathf.RoundToInt(Mathf.Lerp(5f, 15f, normalizedHole)),
            5,
            15
        );

        float explosionForce = blastBehindWall * 2500f;

        //Debug.Log($" Hole Diameter: {holeDiameter} Hole normalll: {normalizedHole} ");

        wallShatter.SetSettingsValues(
            normalizedHole,
            iterations,
            explosionForce,
            blastRadius
        );

        ExplosionChange?.Invoke(Mathf.RoundToInt(explosiveGrams), blastRadius);
    }
    public void OnResetPressed()
    {
        wallShatter.ResetWall();
    }


    public float CalculateHoleDiameter(float explosiveGrams, float wallThicknessCm)
    {
        // 1. Convert explosive to kg
        float E = explosiveGrams / 1000f;

        // 2. Base factor for calculation (like before)
        float baseDiameter = 37f;

        // --- Compute min and max possible holes ---
        float E_min = explosiveMin / 1000f;
        float E_max = explosiveMax / 1000f;

        float minHole = baseDiameter * Mathf.Pow(E_min, 1f / 3f) * (1f - (thicknessMax / 100f));
        float maxHole = baseDiameter * Mathf.Pow(E_max, 1f / 3f) * (1f - (thicknessMin / 100f));

        // 3. Compute current hole
        float currentHole = baseDiameter * Mathf.Pow(E, 1f / 3f) * (1f - (wallThicknessCm / 100f));

        // 4. Normalize between min and max
        float normalized = (currentHole - minHole) / (maxHole - minHole);
        normalized = Mathf.Clamp01(normalized);

        // 5. Apply non-linear curve
        float curved = Mathf.Pow(normalized, 1.5f);

        // 6. Map to actual hole size
        float holeDiameter = Mathf.Lerp(27f, 62f, curved);

        return holeDiameter;
    }

    public float CalculateBlastBehindWall(float explosiveGrams, float wallThicknessCm)
    {
        float E = explosiveGrams / 1000f;
        float k = 0.09f;

        // Calculate min and max possible blast (for normalization)
        float minBlast = (explosiveMin / 1000f) * Mathf.Exp(-k * thicknessMax);
        float maxBlast = (explosiveMax / 1000f) * Mathf.Exp(-k * thicknessMin);

        // Current blast value
        float currentBlast = E * Mathf.Exp(-k * wallThicknessCm);

        // Normalize to 0..1
        float normalized = (currentBlast - minBlast) / (maxBlast - minBlast);

        // Clamp just in case
        normalized = Mathf.Clamp01(normalized);

        // Map to 0.1 - 1
        float mapped = Mathf.Lerp(0.1f, 1f, normalized);
        return mapped;
    }


}