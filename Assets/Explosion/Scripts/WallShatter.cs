using UnityEngine;
using EzySlice;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles wall shattering, including creating a hole and recursively breaking fragments.
/// </summary>
public class WallShatter : MonoBehaviour
{
    public static WallShatter Instance;
    [Header("Wall Reference")]
    [SerializeField] private Transform _FragmentsParent;

    [Header("Shatter Settings")]
    [SerializeField] private Material _CrossSectionMaterial;
    [SerializeField] private float _Force = 0.5f;
    [SerializeField] private int _Iterations = 4;
    [SerializeField] private float _ExplosionForce = 300f;
    [SerializeField] private float _ExplosionRadius = 2f;

    [Header("Impact Particles")]
    [SerializeField] private ParticleSystem _ExplodeParticleSys;
    [SerializeField] private ParticleSystem _StonesParticleSys;
    [SerializeField] private ParticleSystem _VolumeEffectParticleSys;
    [SerializeField] private float _MinVolume = 2f;
    [SerializeField] private float _MaxVolume = 7f;

    [Header("Slow Motion")]
    [SerializeField] private bool _UseSlowMotion = true;
    [SerializeField] private float _SlowMotionScale = 0.2f;
    [SerializeField] private float _SlowMotionDuration = 0.5f;

    [Header("External Tools")]
    [SerializeField] private ProBuilderBooleanTool _ProBuilderTool;
    [SerializeField] private GameObject _SubMeshPrefab;
    [SerializeField] private float _MinScale = 2f;
    [SerializeField] private float _MaxScale = 7f;

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private GameObject _currentVolume;

    private GameObject _currentHole;
    private GameObject _currentRemaning;

    private GameObject _CurrentWall;

    private void Start()
    {
        Instance = this;
    }

    public void ShatterWall(GameObject wall, Vector3 hitPoint, Vector3 hitNormal)
    {
        _CurrentWall = wall;
        _originalPosition = _CurrentWall.transform.position;
        _originalRotation = _CurrentWall.transform.rotation;

        if (_UseSlowMotion)
            StartCoroutine(SlowMotionRoutine());

        if (_SubMeshPrefab != null)
        {
            GameObject spawnedSubMesh = Instantiate(
                _SubMeshPrefab,
                hitPoint,
                Quaternion.LookRotation(hitNormal)
            );

            spawnedSubMesh.transform.SetParent(_FragmentsParent);

            float scale = Mathf.RoundToInt(Mathf.Lerp(_MinScale, _MaxScale, _Force));
            spawnedSubMesh.transform.localScale = new Vector3(scale, scale, 25);

            (_currentRemaning, _currentHole) =
                _ProBuilderTool.CutHoleWithResult(wall, spawnedSubMesh);

            SpawnImpactParticles(hitPoint, hitNormal);

            if (_currentHole != null)
                ShatterRecursive(_currentHole, hitPoint, hitNormal, _Iterations);

            _currentVolume = Instantiate(
                _VolumeEffectParticleSys,
                hitPoint,
                Quaternion.identity
            ).gameObject;

            float scaleValue = Mathf.Lerp(_MinVolume, _MaxVolume, _Force);
            _currentVolume.transform.localScale =
                new Vector3(scaleValue, scaleValue, scaleValue);
        }
    }

    public void ResetWall()
    {
        Time.timeScale = 1f;

        foreach (Transform t in _FragmentsParent)
            Destroy(t.gameObject);

        Destroy(_currentVolume);
        Destroy(_currentRemaning);
        Destroy(_currentHole);

        _CurrentWall.transform.position = _originalPosition;
        _CurrentWall.transform.rotation = _originalRotation;
        _CurrentWall.SetActive(true);
    }

    private IEnumerator SlowMotionRoutine()
    {
        float originalTimeScale = 1f;
        float timer = 0f;

        while (timer < 0.1f)
        {
            timer += Time.unscaledDeltaTime;
            Time.timeScale =
                Mathf.Lerp(originalTimeScale, _SlowMotionScale, timer / 0.1f);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(_SlowMotionDuration);

        timer = 0f;

        while (timer < 0.2f)
        {
            timer += Time.unscaledDeltaTime;
            Time.timeScale =
                Mathf.Lerp(_SlowMotionScale, originalTimeScale, timer / 0.2f);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            yield return null;
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    private void ShatterRecursive(
        GameObject obj,
        Vector3 hitPoint,
        Vector3 hitNormal,
        int iterationsLeft
    )
    {
        if (iterationsLeft <= 0) return;

        Vector3 planeOrigin = hitPoint + Random.insideUnitSphere;
        Vector3 planeNormal = Random.onUnitSphere;

        GameObject[] slices =
            obj.SliceInstantiate(planeOrigin, planeNormal, _CrossSectionMaterial);

        if (slices == null) return;

        foreach (GameObject piece in slices)
        {
            piece.transform.SetParent(_FragmentsParent);
            AddPhysics(piece, hitPoint, hitNormal);

            ShatterRecursive(piece, hitPoint, hitNormal, iterationsLeft - 1);
        }

        //SpawnImpactParticles(hitPoint, hitNormal);

        obj.SetActive(false);
    }

    private void SpawnImpactParticles(Vector3 position, Vector3 normal)
    {
        float yRotation = Mathf.Atan2(normal.x, normal.z) * Mathf.Rad2Deg;

        Quaternion rot = Quaternion.Euler(90f, yRotation, 0f);

        if (_ExplodeParticleSys != null)
        {
            ParticleSystem explode = Instantiate(_ExplodeParticleSys, position, rot);
            explode.Play();
            Destroy(explode.gameObject, explode.main.duration);
        }

        if (_StonesParticleSys != null)
        {
            ParticleSystem stones = Instantiate(_StonesParticleSys, position, rot);
            stones.Play();
            Destroy(stones.gameObject, stones.main.duration);
        }
    }

    private void AddPhysics(GameObject piece, Vector3 hitPoint, Vector3 hitNormal)
    {
        Rigidbody rb = piece.AddComponent<Rigidbody>();
        rb.mass = 1f;

        MeshCollider col = piece.AddComponent<MeshCollider>();
        col.convex = true;

        // Small explosion spread
        rb.AddExplosionForce(_ExplosionForce, hitPoint, _ExplosionRadius);

        // Direction away from wall (projectile direction)
        Vector3 impactDir = -hitNormal;

        // Small randomness
        impactDir = (impactDir + Random.insideUnitSphere * 0.05f).normalized;

        // Much smaller force
        float directionalForce = _ExplosionForce * 0.01f;

        rb.AddForce(impactDir * directionalForce, ForceMode.Impulse);
    }

    private void AddPhysics(GameObject piece, Vector3 hitPoint)
    {
        Rigidbody rb = piece.AddComponent<Rigidbody>();
        rb.mass = 1f;

        rb.AddExplosionForce(_ExplosionForce, hitPoint, _ExplosionRadius);

        MeshCollider col = piece.AddComponent<MeshCollider>();
        col.convex = true;
    }

    public void SetSettingsValues(
        float force,
        int iterations,
        float explosionForce,
        float explosionRadius
    )
    {
        _Force = force;
        _Iterations = iterations;
        _ExplosionForce = explosionForce;
        _ExplosionRadius = explosionRadius;
    }
}