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

    float Map(float value, float inMin, float inMax, float outMin, float outMax)
    {
        return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
    }

    private void UpdateShatterValues(float value)
    {
        float explosiveGrams = Map(_ExplosionSlider.GetCurrentValue(), 0f, 10f, 200f, 2000f);
        float wallThicknessCm = Map(_ThicknessSlider.GetCurrentValue(), 0f, 10f, 5f, 40f);

        float holeDiameter = CalculateHoleDiameter(explosiveGrams, wallThicknessCm);
        float blastBehindWall = CalculateBlastBehindWall(explosiveGrams, wallThicknessCm);


        float normalizedHole = Mathf.InverseLerp(27f, 62f, holeDiameter);

        Debug.Log(" hole " + holeDiameter + "  " + normalizedHole);
        int iterations = Mathf.Clamp(
            Mathf.RoundToInt(Mathf.Lerp(5f, 15f, normalizedHole)),
            5,
            15
        );

        float explosionForce = blastBehindWall * 2500f;

        float blastRadius = Mathf.Lerp(2f, 6f, blastBehindWall);
        //Debug.Log($"Blast Behind Wall: {blastBehindWall}  Hole Diameter: {holeDiameter} cm  Blast force {explosionForce}  Blast radius {blastRadius}");

        wallShatter.SetSettingsValues(
            normalizedHole,
            iterations,
            explosionForce,
            blastRadius
        );

        ExplosionChange?.Invoke(Mathf.RoundToInt(explosiveGrams));
    }
    public void OnResetPressed()
    {
        wallShatter.ResetWall();
    }


    public float CalculateHoleDiameter(float explosiveGrams, float wallThicknessCm)
    {
        float E = explosiveGrams / 1000f;

        float baseDiameter = 37f;
        float hole = baseDiameter * Mathf.Pow(E, 1f / 3f);

        float thicknessFactor = 1f - (wallThicknessCm / 100f);

        hole *= thicknessFactor;

        return Mathf.Clamp(hole, 27f, 62f);
    }

    public float CalculateBlastBehindWall(float explosiveGrams, float wallThicknessCm)
    {
        float E = explosiveGrams / 1000f;

        float k = 0.07f;

        return E * Mathf.Exp(-k * wallThicknessCm);
    }


}