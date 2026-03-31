using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;
using System;
using Random = UnityEngine.Random;

public class CarShatter : MonoBehaviour
{

    [SerializeField] private List<ShatterObjInfo> _ShatterObjs;
    [SerializeField] private Transform _FragmentsParent;
    public static Action<Vector3, Vector3, float, float> ShatterObjs;

    [SerializeField] private float _ExplosionForce = 300f;
    [SerializeField] private float _ExplosionRadius = 2f;

    private void OnEnable()
    {
        ShatterObjs += ShatterCar;
    }

    private void OnDisable()
    {
        ShatterObjs -= ShatterCar; 
    }

    private void ShatterCar(
        Vector3 hitPoint,
        Vector3 hitNormal,
        float force,
        float radius)
    {
        //_ExplosionForce = force;
        //_ExplosionRadius = radius;
        foreach(ShatterObjInfo shatter in _ShatterObjs)
        {
            ShatterRecursive(shatter.obj, hitPoint, hitNormal, Random.Range(7, 10), shatter.material);
        }
    }
    public void ShatterRecursive(
        GameObject obj,
        Vector3 hitPoint,
        Vector3 hitNormal,
        int iterationsLeft,
        Material mat
    )
    {
        if (iterationsLeft <= 0) return;
        Vector3 planeOrigin = hitPoint + Random.insideUnitSphere;
        Vector3 planeNormal = Random.onUnitSphere;

        GameObject[] slices =
            obj.SliceInstantiate(planeOrigin, planeNormal, mat);

        if (slices == null)
        {
            if (!obj.TryGetComponent<Rigidbody>(out _))
            {
                Rigidbody rb = obj.AddComponent<Rigidbody>();
                Vector3 forceDirection = Vector3.forward; // replace with the direction you want
                float forceAmount = 500; // adjust as needed
                rb.AddForce(forceDirection * forceAmount, ForceMode.Impulse);
            }

            return;
        }

        foreach (GameObject piece in slices)
        {
            piece.transform.SetParent(_FragmentsParent);
            AddPhysics(piece, hitPoint, hitNormal);

            ShatterRecursive(piece, hitPoint, hitNormal, iterationsLeft - 1, mat);
        }

        //SpawnImpactParticles(hitPoint, hitNormal);

        obj.SetActive(false);
    }

    private void AddPhysics(GameObject piece, Vector3 hitPoint, Vector3 hitNormal)
    {
        Rigidbody rb = piece.AddComponent<Rigidbody>();
        rb.mass = 1f;
        MeshCollider col = piece.AddComponent<MeshCollider>();
        col.convex = true;
        rb.AddExplosionForce(_ExplosionForce, hitPoint, _ExplosionRadius);
        Vector3 impactDir = -hitNormal;
        impactDir = (impactDir + Random.insideUnitSphere * 0.05f).normalized;
        float directionalForce = _ExplosionForce * 0.01f;

        rb.AddForce(impactDir * directionalForce, ForceMode.Impulse);
    }

    public EzySlice.Plane GetRandomPlane(Vector3 positionOffset, Vector3 scaleOffset)
    {
        Vector3 randomPosition = Random.insideUnitSphere;

        randomPosition += positionOffset;

        Vector3 randomDirection = Random.insideUnitSphere.normalized;

        return new EzySlice.Plane(randomPosition, randomDirection);
    }

}

[System.Serializable]
public class ShatterObjInfo
{
    public GameObject obj;
    public Material material;

}