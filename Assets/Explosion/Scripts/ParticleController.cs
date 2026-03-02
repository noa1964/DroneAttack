using UnityEngine;

public class ParticleController : MonoBehaviour
{
    [SerializeField] private ParticleSystem ps;

    [Header("Size Range (X = min, Y = max)")]
    [SerializeField] private Vector2 minSizeRange = new Vector2(0.2f, 0.5f);
    [SerializeField] private Vector2 maxSizeRange = new Vector2(1.5f, 2.5f);

    [Header("Speed Range (X = min, Y = max)")]
    [SerializeField] private Vector2 minSpeedRange = new Vector2(0.2f, 0.5f);
    [SerializeField] private Vector2 maxSpeedRange = new Vector2(1.5f, 2.5f);

    [Header("Emission")]
    [SerializeField] private int minEmission = 5;
    [SerializeField] private int maxEmission = 20;

    private void OnEnable()
    {
        ShatterUIController.ForceChange += UpdateParticlesByIntensity;
    }

    private void OnDisable()
    {
        ShatterUIController.ForceChange -= UpdateParticlesByIntensity;
    }

    /// <summary>
    /// Updates particle system based on intensity (1–10)
    /// Applies start size, start speed, and emission burst count
    /// </summary>
    public void UpdateParticlesByIntensity(int intensity)
    {
        intensity = Mathf.Clamp(intensity, 1, 10);

        // Normalize 1–10 to 0–1
        float t = (intensity - 1) / 9f;

        // ===== START SIZE =====
        float sizeMin = Mathf.Lerp(minSizeRange.x, maxSizeRange.x, t);
        float sizeMax = Mathf.Lerp(minSizeRange.y, maxSizeRange.y, t);
        var main = ps.main;
        main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);

        // ===== START SPEED =====
        float speedMin = Mathf.Lerp(minSpeedRange.x, maxSpeedRange.x, t);
        float speedMax = Mathf.Lerp(minSpeedRange.y, maxSpeedRange.y, t);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speedMin, speedMax);

        // ===== EMISSION BURST =====
        var emission = ps.emission;
        if (emission.burstCount > 0)
        {
            // Assuming first burst controls count
            ParticleSystem.Burst burst = emission.GetBurst(0);
            int newCount = Mathf.RoundToInt(Mathf.Lerp(minEmission, maxEmission, t));
            burst.count = newCount;
            emission.SetBurst(0, burst);
        }
    }
}