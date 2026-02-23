using UnityEngine;

public class DirectionalExplosion : MonoBehaviour
{
    public ParticleSystem[] particleSystems;
    public Vector3 direction;

    public void Explode()
    {
        // Normalize direction
        direction.Normalize();

        // Rotate explosion object to face the direction
        transform.rotation = Quaternion.LookRotation(direction);

        // Play all particle systems

    }
}