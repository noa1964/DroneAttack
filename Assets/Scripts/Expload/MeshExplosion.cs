using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshExplosion : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionForce = 800f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float upwardsModifier = 0.3f;

    [Header("Force Randomness")]
    [SerializeField] private float minForceMultiplier = 0.8f;
    [SerializeField] private float maxForceMultiplier = 1.2f;

    [Header("Fragment Settings")]
    [SerializeField, Range(0, 3)] private int subdivisionLevel = 1;
    [SerializeField] private float torqueMultiplier = 5f;
    [SerializeField] private float fragmentScale = 1f;
    [SerializeField] private float fragmentLifetime = 5f;

    [Header("Fragment Chaos Settings")]
    [SerializeField, Range(0f, 0.3f)] private float vertexChaos = 0.05f; // random vertex offset
    [SerializeField, Range(0f, 1f)] private float holeChance = 0.05f;   // chance triangle disappears

    [Header("Explosion Origin")]
    [SerializeField] private Transform customOrigin;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void OnMouseDown()
    {
        Vector3 origin = customOrigin != null ? customOrigin.position : transform.position;
        Explode(origin);
    }

    public void Explode(Vector3 origin)
    {
        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;

        List<int>[] newSubMeshTriangles = new List<int>[mesh.subMeshCount];
        for (int i = 0; i < newSubMeshTriangles.Length; i++)
            newSubMeshTriangles[i] = new List<int>();

        for (int sub = 0; sub < mesh.subMeshCount; sub++)
        {
            int[] triangles = mesh.GetTriangles(sub);

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 v0 = vertices[triangles[i]];
                Vector3 v1 = vertices[triangles[i + 1]];
                Vector3 v2 = vertices[triangles[i + 2]];

                // Convert to world space
                Vector3 worldV0 = transform.TransformPoint(v0);
                Vector3 worldV1 = transform.TransformPoint(v1);
                Vector3 worldV2 = transform.TransformPoint(v2);
                Vector3 center = (worldV0 + worldV1 + worldV2) / 3f;

                // Check if this triangle should explode
                if (Vector3.Distance(center, origin) <= explosionRadius)
                {
                    // Subdivide and create fragments
                    SubdivideTriangle(v0, v1, v2, meshRenderer.materials[sub], origin);
                }
                else
                {
                    // Keep triangles outside the explosion
                    newSubMeshTriangles[sub].Add(triangles[i]);
                    newSubMeshTriangles[sub].Add(triangles[i + 1]);
                    newSubMeshTriangles[sub].Add(triangles[i + 2]);
                }
            }
        }

        // Apply new triangles to original mesh to create "holes"
        for (int sub = 0; sub < mesh.subMeshCount; sub++)
        {
            mesh.SetTriangles(newSubMeshTriangles[sub].ToArray(), sub);
        }

        mesh.RecalculateNormals();
    }

    private void SubdivideTriangle(Vector3 v0, Vector3 v1, Vector3 v2, Material material, Vector3 origin)
    {
        List<(Vector3, Vector3, Vector3)> tris = new List<(Vector3, Vector3, Vector3)>();
        tris.Add((v0, v1, v2));

        for (int s = 0; s < subdivisionLevel; s++)
        {
            List<(Vector3, Vector3, Vector3)> newTris = new List<(Vector3, Vector3, Vector3)>();

            foreach (var tri in tris)
            {
                Vector3 a = tri.Item1;
                Vector3 b = tri.Item2;
                Vector3 c = tri.Item3;

                // Randomly offset midpoints
                Vector3 ab = (a + b) / 2 + Random.insideUnitSphere * vertexChaos;
                Vector3 bc = (b + c) / 2 + Random.insideUnitSphere * vertexChaos;
                Vector3 ca = (c + a) / 2 + Random.insideUnitSphere * vertexChaos;

                // Center point with random offset
                Vector3 center = (a + b + c) / 3 + Random.insideUnitSphere * vertexChaos;

                // Create messy sub-triangles
                newTris.Add((a, ab, center));
                newTris.Add((ab, b, center));
                newTris.Add((b, bc, center));
                newTris.Add((bc, c, center));
                newTris.Add((c, ca, center));
                newTris.Add((ca, a, center));
            }

            tris = newTris;
        }

        // Create fragments
        foreach (var tri in tris)
        {
            if (Random.value < holeChance) continue; // optional holes
            CreateFragment(tri.Item1, tri.Item2, tri.Item3, material, origin);
        }
    }

    private void CreateFragment(Vector3 v0, Vector3 v1, Vector3 v2, Material material, Vector3 origin)
    {
        Mesh m = new Mesh();
        m.vertices = new Vector3[] { v0, v1, v2 };
        m.triangles = new int[] { 0, 1, 2 };
        m.RecalculateNormals();

        GameObject fragment = new GameObject("Fragment");
        fragment.transform.position = transform.position;
        fragment.transform.rotation = transform.rotation;
        fragment.transform.localScale = transform.localScale * fragmentScale;

        fragment.AddComponent<MeshFilter>().mesh = m;
        fragment.AddComponent<MeshRenderer>().material = material;
        fragment.AddComponent<BoxCollider>();

        Rigidbody rb = fragment.AddComponent<Rigidbody>();

        // Randomized explosion force
        float randomForce = explosionForce * Random.Range(minForceMultiplier, maxForceMultiplier);
        rb.AddExplosionForce(randomForce, origin, explosionRadius, upwardsModifier, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * torqueMultiplier, ForceMode.Impulse);

        Destroy(fragment, fragmentLifetime);
    }
}
