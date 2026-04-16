using UnityEngine;
using EzySlice;
using System.Collections.Generic;

public class WindowShatter : MonoBehaviour
{
    [SerializeField] private List<GameObject> _WindowsList;
    [Header("Shatter Settings")]
    [SerializeField] private Material crossSectionMaterial;
    [SerializeField] private int minIterations = 2;
    [SerializeField] private int maxIterations = 5;

    [Header("Explosion Settings")]
    [SerializeField] private float explosionForce = 300f;
    [SerializeField] private float explosionRadius = 2f;

    [Header("Parent")]
    [SerializeField] private Transform fragmentsParent;

    /// <summary>
    /// Call this to shatter any object (like a window)
    /// </summary>
    public void Shatter()
    {
        foreach (GameObject window in _WindowsList)
        {

            int iterations = Random.Range(minIterations, maxIterations + 1);

            Vector3 hitPoint = window.transform.position;
            Vector3 hitNormal = Random.onUnitSphere;

            ShatterRecursive(window, hitPoint, hitNormal, iterations);
        }
    }

    private void ShatterRecursive(GameObject obj, Vector3 hitPoint, Vector3 hitNormal, int iterationsLeft)
    {
        if (iterationsLeft <= 0)
        {
            AddPhysics(obj, hitPoint);
            return;
        }

        Vector3 planeOrigin = obj.transform.position + Random.insideUnitSphere * 0.2f;
        Vector3 planeNormal = Random.onUnitSphere;

        GameObject[] pieces = obj.SliceInstantiate(planeOrigin, planeNormal, crossSectionMaterial);

        if (pieces == null)
        {
            AddPhysics(obj, hitPoint);
            return;
        }

        foreach (GameObject piece in pieces)
        {
            piece.transform.SetParent(fragmentsParent, true);
            ShatterRecursive(piece, hitPoint, hitNormal, iterationsLeft - 1);
        }

        obj.SetActive(false);
    }

    private void AddPhysics(GameObject piece, Vector3 explosionPoint)
    {
        Rigidbody rb = piece.AddComponent<Rigidbody>();
        rb.mass = 1f;

        MeshCollider col = piece.AddComponent<MeshCollider>();
        col.convex = true;

        rb.AddExplosionForce(explosionForce, explosionPoint, explosionRadius);
    }
}