using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Static class which emulates a mesh explosion into triangles
/// 
/// @autor Alex M.A.
/// </summary>
public class MeshTriangulator
{
    public static float explosionForce = 800f;
    public static float explosionRadius = 5f;
    public static float upwardsModifier = 0.3f;

    public static float torqueMultiplier = 5f;

    public static float minForceMultiplier = 0.7f;
    public static float maxForceMultiplier = 1.3f;

    public static float fragmentLifetime = 5f;

    [Range(1, 5)]
    public static int subdivisionLevel = 1; // Controls amount of fragments

    public static float fragmentScale = 1f;

    // ============================================

    public static void Explode(
        Transform target,
        Vector3? customOrigin = null)
    {
        MeshFilter meshFilter = target.GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = target.GetComponent<MeshRenderer>();

        if (meshFilter == null) return;

        Mesh mesh = meshFilter.mesh;

        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        Vector2[] uvs = mesh.uv;

        Vector3 explosionOrigin = customOrigin ?? target.position;

        target.gameObject.SetActive(false);

        for (int sub = 0; sub < mesh.subMeshCount; sub++)
        {
            int[] triangles = mesh.GetTriangles(sub);

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 v0 = vertices[triangles[i]];
                Vector3 v1 = vertices[triangles[i + 1]];
                Vector3 v2 = vertices[triangles[i + 2]];

                SubdivideAndCreate(
                    v0, v1, v2,
                    normals[triangles[i]],
                    normals[triangles[i + 1]],
                    normals[triangles[i + 2]],
                    uvs[triangles[i]],
                    uvs[triangles[i + 1]],
                    uvs[triangles[i + 2]],
                    meshRenderer.materials[sub],
                    target,
                    explosionOrigin
                );
            }
        }
    }

    private static void SubdivideAndCreate(
        Vector3 v0, Vector3 v1, Vector3 v2,
        Vector3 n0, Vector3 n1, Vector3 n2,
        Vector2 uv0, Vector2 uv1, Vector2 uv2,
        Material material,
        Transform original,
        Vector3 explosionOrigin)
    {
        List<(Vector3, Vector3, Vector3)> triangles = new();

        triangles.Add((v0, v1, v2));

        // Subdivide
        for (int s = 0; s < subdivisionLevel; s++)
        {
            List<(Vector3, Vector3, Vector3)> newTris = new();

            foreach (var tri in triangles)
            {
                Vector3 a = tri.Item1;
                Vector3 b = tri.Item2;
                Vector3 c = tri.Item3;

                Vector3 ab = (a + b) / 2;
                Vector3 bc = (b + c) / 2;
                Vector3 ca = (c + a) / 2;

                newTris.Add((a, ab, ca));
                newTris.Add((ab, b, bc));
                newTris.Add((ca, bc, c));
                newTris.Add((ab, bc, ca));
            }

            triangles = newTris;
        }

        foreach (var tri in triangles)
        {
            CreateTriangle(tri.Item1, tri.Item2, tri.Item3,
                material, original, explosionOrigin);
        }
    }

    private static void CreateTriangle(
        Vector3 v0, Vector3 v1, Vector3 v2,
        Material material,
        Transform original,
        Vector3 explosionOrigin)
    {
        Mesh m = new Mesh();
        m.vertices = new Vector3[] { v0, v1, v2 };
        m.triangles = new int[] { 0, 1, 2 };
        m.RecalculateNormals();

        GameObject go = new GameObject("Fragment");
        go.transform.position = original.position;
        go.transform.rotation = original.rotation;
        go.transform.localScale = original.localScale * fragmentScale;

        go.AddComponent<MeshFilter>().mesh = m;
        go.AddComponent<MeshRenderer>().material = material;

        go.AddComponent<BoxCollider>();

        Rigidbody rb = go.AddComponent<Rigidbody>();

        float randomForce =
            explosionForce *
            Random.Range(minForceMultiplier, maxForceMultiplier);

        rb.AddExplosionForce(
            randomForce,
            explosionOrigin,
            explosionRadius,
            upwardsModifier,
            ForceMode.Impulse);

        rb.AddTorque(Random.insideUnitSphere * torqueMultiplier,
            ForceMode.Impulse);

        Object.Destroy(go, fragmentLifetime);
    }
}