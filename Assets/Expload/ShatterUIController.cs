using UnityEngine;

public class ShatterUIController : MonoBehaviour
{
    [SerializeField] private WallShatter wallShatter;
    [SerializeField] private Projectile projectile;

    [SerializeField] private SliderWithLabel iterationsSlider;
    [SerializeField] private SliderWithLabel radiusSlider;
    [SerializeField] private SliderWithLabel forceSlider;
    [SerializeField] private SliderWithLabel explosionRadiusSlider;

    private void Start()
    {
        iterationsSlider.OnValueChanged += value => wallShatter.iterations = (int)value;
        radiusSlider.OnValueChanged += value => wallShatter.radius = value;
        forceSlider.OnValueChanged += value => wallShatter.explosionForce = value;
        explosionRadiusSlider.OnValueChanged += value => wallShatter.explosionRadius = value;
    }

    public void OnResetPressed()
    {
        wallShatter.ResetWall();
        projectile.ResetProjectile();
    }
}