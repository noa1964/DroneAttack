using System.Collections.Generic;
using UnityEngine;

public class RuntimeMeshExploder : MonoBehaviour
{
    [Header("Explosion Settings")]
    public int slices = 3;              // How many random cuts
    public float explosionForce = 300f; // Force applied to pieces
    public float explosionRadius = 5f;

    // Call this to slice and explode
    public void Explode()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh originalMesh = mf.mesh;

        // Initial list of pieces contains just the original mesh
        List<Mesh> pieces = new List<Mesh> { originalMesh };

        // Generate random planes and slice meshes
        for (int i = 0; i < slices; i++)
        {
            List<Mesh> newPieces = new List<Mesh>();
            foreach (var mesh in pieces)
            {
                Plane randomPlane = GenerateRandomPlane();
                // Slice returns 2 pieces (can be null if plane doesn't intersect)
                Mesh[] split = SliceMesh(mesh, randomPlane);
                if (split != null)
                {
                    newPieces.AddRange(split);
                }
                else
                {
                    newPieces.Add(mesh);
                }
            }
            pieces = newPieces;
        }

        // Spawn pieces with rigidbodies
        foreach (var mesh in pieces)
        {
            GameObject go = new GameObject("Piece");
            go.transform.position = transform.position;
            go.transform.rotation = transform.rotation;

            MeshFilter mfPiece = go.AddComponent<MeshFilter>();
            mfPiece.mesh = mesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.materials = GetComponent<MeshRenderer>().materials;

            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
        }

        // Destroy original object
        Destroy(gameObject);
    }

    private Plane GenerateRandomPlane()
    {
        Vector3 dir = Random.onUnitSphere; // Random normal
        Vector3 point = transform.position; // Center plane at object
        return new Plane(dir, point);
    }

    // Placeholder slice method: you need actual mesh slicing here
    private Mesh[] SliceMesh(Mesh mesh, Plane plane)
    {
        // ---- VERY IMPORTANT ----
        // This is where you implement triangle-plane intersection math
        // There are open-source runtime mesh slicers for Unity you can copy:
        // Example: EzySlice (MIT License): https://github.com/DavidArayan/EzySlice
        // It already handles runtime slicing, normals, UVs, etc.
        return new Mesh[] { mesh }; // Temporary: returns original mesh (replace with real slice)
    }
}