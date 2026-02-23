using UnityEngine;
using EzySlice;

public class WallShatter : MonoBehaviour
{
    [Header("Wall Reference")]
    [SerializeField] private GameObject currentWall;
    [SerializeField] private Transform fragmentsParent;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    [Header("Shatter Settings")]
    [SerializeField] private Material crossSectionMaterial;

     public int iterations = 4;
     public float radius = 0.5f;
     public float explosionForce = 300f;
     public float explosionRadius = 2f;

    [Header("Impact Particles")]
    [SerializeField] private ParticleSystem explode;
    [SerializeField] private ParticleSystem stones;

    private void Start()
    {
        originalPosition = currentWall.transform.position;
        originalRotation = currentWall.transform.rotation;

        currentWall.tag = "Wall";
    }

    // Called from Projectile
    public void ShatterWall(GameObject wall, Vector3 hitPoint, Vector3 hitNormal)
    {
        SpawnImpactParticles(hitPoint, hitNormal);

        ShatterRecursive(wall, hitPoint, iterations);
    }

    public void ResetWall()
    {
        // Destroy fragments
        foreach (Transform t in fragmentsParent)
        {
            Destroy(t.gameObject);
        }

        // Restore original wall
        currentWall.transform.position = originalPosition;
        currentWall.transform.rotation = originalRotation;
        currentWall.SetActive(true);
    }

    private void ShatterRecursive(GameObject obj, Vector3 hitPoint, int iterationsLeft)
    {
        if (iterationsLeft <= 0)
            return;

        Vector3 planeOrigin = hitPoint + Random.insideUnitSphere * radius;
        Vector3 planeNormal = Random.onUnitSphere;

        GameObject[] slices = obj.SliceInstantiate(planeOrigin, planeNormal, crossSectionMaterial);

        if (slices == null)
            return;

        foreach (GameObject piece in slices)
        {
            piece.transform.SetParent(fragmentsParent);

            AddPhysics(piece, hitPoint);

            ShatterRecursive(piece, hitPoint, iterationsLeft - 1);
        }

        explode.Play();
        stones.Play();
        obj.SetActive(false);
    }

    private void AddPhysics(GameObject piece, Vector3 hitPoint)
    {
        Rigidbody rb = piece.AddComponent<Rigidbody>();
        rb.mass = 1f;

        rb.AddExplosionForce(explosionForce, hitPoint, explosionRadius);

        MeshCollider col = piece.AddComponent<MeshCollider>();
        col.convex = true;
    }

    private void SpawnImpactParticles(Vector3 position, Vector3 normal)
    {
        //explode.Play();
        //stones.Play();

        // Rotate particle so Z+ faces outward from surface
        //particle.transform.rotation = Quaternion.LookRotation(normal);

        //Destroy(particle, 3f);
    }
}