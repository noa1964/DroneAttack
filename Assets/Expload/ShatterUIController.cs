using System;
using UnityEngine;

public class ShatterUIController : MonoBehaviour
{
    [SerializeField] private WallShatter wallShatter;
    [SerializeField] private Projectile projectile;

    [SerializeField] private SliderWithLabel _ForceSlider;

    [Header("Ranges")]
    [SerializeField] private int iterationsMin = 1;
    [SerializeField] private int iterationsMax = 6;

    [SerializeField] private float explosionForceMin = 100f;
    [SerializeField] private float explosionForceMax = 500f;

    [SerializeField] private float explosionRadiusMin = 1f;
    [SerializeField] private float explosionRadiusMax = 5f;

    public static Action<int> ForceChange;

    private void Start()
    {
        _ForceSlider.OnValueChanged += UpdateShatterValues;
    }

    private void UpdateShatterValues(float sliderValue)
    {
        // Assume sliderValue is 0-1 normalized
        float t = (sliderValue)/10;

        // Update wall shatter parameters based on slider
        wallShatter.iterations = Mathf.RoundToInt(Mathf.Lerp(iterationsMin, iterationsMax, t));
        wallShatter.explosionForce = Mathf.Lerp(explosionForceMin, explosionForceMax, t);
        wallShatter.explosionRadius = Mathf.Lerp(explosionRadiusMin, explosionRadiusMax, t);
        wallShatter.Force = t; // store normalized intensity for particles, etc.

        // Notify particle controllers or other listeners
        ForceChange?.Invoke(Mathf.RoundToInt(t * 10)); // passes 1-10
    }

    public void OnResetPressed()
    {
        wallShatter.ResetWall();
        projectile.ResetProjectile();
    }
}