using System;
using UnityEngine;

/// <summary>
/// Reads two UI sliders (explosive amount and wall thickness), maps them to
/// physical values, computes hole diameter and blast intensity, and applies
/// those values to a `WallShatter` instance. Exposes an event for UI updates.
/// </summary>
public class ShatterUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WallShatter wallShatter;

    [Header("Sliders")]
    [SerializeField] private SliderWithLabel _ExplosionSlider;   // slider value range (see _SliderValueMin/_SliderValueMax)
    [SerializeField] private SliderWithLabel _ThicknessSlider;   // slider value range (see _SliderValueMin/_SliderValueMax)

    [Header("Slider value range (if sliders are normalized)")]
    [SerializeField] private float _SliderValueMin = 1f;
    [SerializeField] private float _SliderValueMax = 10f;

    [Header("Real Ranges (physical units)")]
    [Tooltip("Explosive mass range (grams)")]
    [SerializeField] private float explosiveMin = 200f;
    [SerializeField] private float explosiveMax = 2000f;

    [Tooltip("Wall thickness range (centimeters)")]
    [SerializeField] private float thicknessMin = 5f;
    [SerializeField] private float thicknessMax = 40f;

    [Header("Hole mapping")]
    [SerializeField] private float holeBaseDiameter = 37f; // base scaling constant used in diameter calculation
    [SerializeField] private float holeOutputMin = 27f; // final hole diameter min (cm)
    [SerializeField] private float holeOutputMax = 62f; // final hole diameter max (cm)
    [SerializeField, Range(0.1f, 3f)] private float holeCurveExponent = 1.5f;

    [Header("Blast mapping")]
    [SerializeField] private float blastK = 0.09f; // attenuation factor used in exponential model
    [SerializeField] private float blastMappedMin = 0.1f;
    [SerializeField] private float blastMappedMax = 1f;
    [SerializeField] private float blastToForceScale = 2500f;
    [SerializeField, Range(0.1f, 3f)] private float blastNonLinearPower = 0.8f; // controls radius curve
    [SerializeField] private float blastRadiusMin = 1f;
    [SerializeField] private float blastRadiusMax = 9f;

    [Header("Fragment iterations")]
    [SerializeField] private int iterationsMin = 5;
    [SerializeField] private int iterationsMax = 15;

    /// <summary>
    /// (explosive grams rounded, blastRadius)
    /// </summary>
    public static Action<int, float> ExplosionChange;

    private void OnEnable()
    {
        if (_ExplosionSlider != null)
            _ExplosionSlider.OnValueChanged += UpdateShatterValues;
        if (_ThicknessSlider != null)
            _ThicknessSlider.OnValueChanged += UpdateShatterValues;

        // initialize UI -> logic
        UpdateShatterValues(0f);
    }

    private void OnDisable()
    {
        if (_ExplosionSlider != null)
            _ExplosionSlider.OnValueChanged -= UpdateShatterValues;
        if (_ThicknessSlider != null)
            _ThicknessSlider.OnValueChanged -= UpdateShatterValues;
    }

    private void Start()
    {
        if (wallShatter == null)
            Debug.LogWarning("ShatterUIController: `wallShatter` is not assigned.", this);

        if (_ExplosionSlider == null || _ThicknessSlider == null)
            Debug.LogWarning("ShatterUIController: one or more sliders are not assigned.", this);
    }

    private static float Map(float value, float inMin, float inMax, float outMin, float outMax)
    {
        if (Mathf.Approximately(inMax, inMin))
            return outMin;
        return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
    }

    /// <summary>
    /// UI change handler. The incoming float comes from the slider event but we
    /// read both sliders to compute the mapped physical values.
    /// </summary>
    private void UpdateShatterValues(float _)
    {
        if (wallShatter == null) return; // nothing to apply to

        // Read raw slider values
        float sliderExplosiveValue = _ExplosionSlider != null ? _ExplosionSlider.GetCurrentValue() : explosiveMin;
        float sliderThicknessValue = _ThicknessSlider != null ? _ThicknessSlider.GetCurrentValue() : thicknessMin;

        // Convert slider units to physical units (grams and cm).
        // If sliders already supply physical units, set _SliderValueMin/_SliderValueMax accordingly.
        //float wallThicknessCm = Map(sliderThicknessValue, _SliderValueMin, _SliderValueMax, thicknessMin, thicknessMax);

        // Compute hole diameter and blast intensity
        float holeDiameter = CalculateHoleDiameter(sliderExplosiveValue, sliderThicknessValue);
        float blastBehindWall = CalculateBlastBehindWall(sliderExplosiveValue, sliderThicknessValue);

        // Normalize hole within expected final diameter range
        float normalizedHole = Mathf.InverseLerp(holeOutputMin, holeOutputMax, holeDiameter);

        // Compute blast radius with a gentle non-linear curve
        float intensity = Mathf.Clamp01(blastBehindWall);
        float nonLinearFactor = Mathf.Pow(intensity, blastNonLinearPower);
        float blastRadius = Mathf.Lerp(blastRadiusMin, blastRadiusMax, nonLinearFactor);

        // Iteration count depends on hole size (more hole -> more fragments)
        int iterations = Mathf.Clamp(
            Mathf.RoundToInt(Mathf.Lerp(iterationsMin, iterationsMax, normalizedHole)),
            iterationsMin,
            iterationsMax
        );

        // Explosion force scales from mapped blast value
        float explosionForce = blastBehindWall * blastToForceScale;

        wallShatter.SetSettingsValues(
            normalizedHole,
            iterations,
            explosionForce,
            blastRadius
        );

        float explosiveGrams = Map(sliderExplosiveValue, explosiveMin, explosiveMax, _SliderValueMin, _SliderValueMax);
       
        ExplosionChange?.Invoke(Mathf.RoundToInt(explosiveGrams), blastRadius);
    }
    public void OnResetPressed()
    {
        wallShatter.ResetWall();
    }


    public float CalculateHoleDiameter(float explosiveGrams, float wallThicknessCm)
    {
        // New behavior:
        // - Thickness controls sensitivity and max cap.
        // - Thick wall -> explosive is very sensitive (steep response) but max capped ~80%.
        // - Thin wall  -> explosive is weak (flat response) but can reach full 100%.
        // Implementation maps inputs to normalized 0..1 and composes baseline + explosive effect scaled by cap.

        // Normalize thickness and explosive to 0..1
        float thicknessNorm = Mathf.InverseLerp(thicknessMin, thicknessMax, wallThicknessCm);
        thicknessNorm = Mathf.Clamp01(thicknessNorm);

        float explosiveNorm = Mathf.InverseLerp(explosiveMin, explosiveMax, explosiveGrams);
        explosiveNorm = Mathf.Clamp01(explosiveNorm);

        // Baseline hole (when explosive is minimum):
        // thin wall -> baseline ~50% of range, thick wall -> baseline ~0%
        float baselineNormalized = Mathf.Lerp(0.5f, 0f, thicknessNorm);

        // Max cap (normalized): thin -> 1.0 (100%), thick -> 0.8 (80%)
        float capNormalized = Mathf.Lerp(1f, 0.8f, thicknessNorm);

        // Sensitivity exponent: thin -> flat (~0.4), thick -> steep (~3.0)
        const float minExponent = 0.4f;
        const float maxExponent = 3.0f;
        // Use serialized holeCurveExponent (0.1..3) as an additional tuning weight
        float curveWeight = Mathf.InverseLerp(0.1f, 3f, holeCurveExponent);
        float exponent = Mathf.Lerp(minExponent, maxExponent, thicknessNorm * curveWeight);

        // Apply non-linear response to explosive slider
        float explosiveEffect = Mathf.Pow(explosiveNorm, exponent);

        // Compose final normalized hole within [baseline, cap]
        float holeNormalized = baselineNormalized + (capNormalized - baselineNormalized) * explosiveEffect;
        holeNormalized = Mathf.Clamp01(holeNormalized);

        // Map normalized hole to actual diameter range
        float holeDiameter = Mathf.Lerp(holeOutputMin, holeOutputMax, holeNormalized);
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