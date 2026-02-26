using UnityEngine;
using EzySlice;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Handles wall shattering, including creating a hole and recursively breaking fragments.
/// </summary>
public class WallShatter : MonoBehaviour
{
    [Header("Wall Reference")]
    [SerializeField] private GameObject _CurrentWall;
    [SerializeField] private Transform _FragmentsParent;

    [Header("Shatter Settings")]
    [SerializeField] private Material _CrossSectionMaterial;
    [SerializeField] private float _Force = 0.5f;
    [SerializeField] private int _Iterations = 4;
    [SerializeField] private float _ExplosionForce = 300f;
    [SerializeField] private float _ExplosionRadius = 2f;
    //[SerializeField] private float _Radius = 0.5f;

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

    #region Unity Callbacks

    private void Start()
    {
        _originalPosition = _CurrentWall.transform.position;
        _originalRotation = _CurrentWall.transform.rotation;
        _CurrentWall.tag = "Wall";
    }

    #endregion

    #region Public Methods

    public void ShatterWall(GameObject wall, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (_UseSlowMotion)
            StartCoroutine(SlowMotionRoutine());

        // --- 1) Place the subMesh prefab at the hit location ---
        if (_SubMeshPrefab != null)
        {
            GameObject spawnedSubMesh = Instantiate(_SubMeshPrefab, hitPoint, Quaternion.LookRotation(hitNormal));
            spawnedSubMesh.transform.SetParent(_FragmentsParent); // optional: keep hierarchy organized

            float scale = Mathf.RoundToInt(Mathf.Lerp(_MinScale, _MaxScale, _Force));
            spawnedSubMesh.transform.localScale = new Vector3(scale, scale, 25);

            // --- 2) Cut hole and get the removed piece ---
            (_currentRemaning, _currentHole) = _ProBuilderTool.CutHoleWithResult(wall, spawnedSubMesh);

            if (_currentHole != null)
                ShatterRecursive(_currentHole, hitPoint, _Iterations);

            // --- 3) Spawn volume effect ---
            _currentVolume = Instantiate(_VolumeEffectParticleSys, hitPoint, Quaternion.identity).gameObject;
            float scaleValue = Mathf.Lerp(_MinVolume, _MaxVolume, _Force);
            _currentVolume.transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
        }
    }

    public void ResetWall()
    {
        Time.timeScale = 1f;

        // Destroy fragments
        foreach (Transform t in _FragmentsParent)
            Destroy(t.gameObject);

        Destroy(_currentVolume);
        Destroy(_currentRemaning);
        Destroy(_currentHole);
        // Restore original wall
        _CurrentWall.transform.position = _originalPosition;
        _CurrentWall.transform.rotation = _originalRotation;
        _CurrentWall.SetActive(true);
    }

    #endregion

    #region Private Methods

    private IEnumerator SlowMotionRoutine()
    {
        float originalTimeScale = 1f;
        float timer = 0f;

        // Smooth slow down
        while (timer < 0.1f)
        {
            timer += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(originalTimeScale, _SlowMotionScale, timer / 0.1f);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(_SlowMotionDuration);

        timer = 0f;

        // Smooth return
        while (timer < 0.2f)
        {
            timer += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(_SlowMotionScale, originalTimeScale, timer / 0.2f);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            yield return null;
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    private void ShatterRecursive(GameObject obj, Vector3 hitPoint, int iterationsLeft)
    {
        if (iterationsLeft <= 0) return;

        Vector3 planeOrigin = hitPoint + Random.insideUnitSphere;
        Vector3 planeNormal = Random.onUnitSphere;

        GameObject[] slices = obj.SliceInstantiate(planeOrigin, planeNormal, _CrossSectionMaterial);

        if (slices == null) return;

        foreach (GameObject piece in slices)
        {
            piece.transform.SetParent(_FragmentsParent);
            AddPhysics(piece, hitPoint);
            ShatterRecursive(piece, hitPoint, iterationsLeft - 1);
        }

        _ExplodeParticleSys.Play();
        _StonesParticleSys.Play();
        obj.SetActive(false);
    }

    private void AddPhysics(GameObject piece, Vector3 hitPoint)
    {
        Rigidbody rb = piece.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.AddExplosionForce(_ExplosionForce, hitPoint, _ExplosionRadius);

        MeshCollider col = piece.AddComponent<MeshCollider>();
        col.convex = true;
    }

    public void SetSettingsValues(float force, int iterations, float explosionForce, float explosionRadius)
    {
        _Force = force;
        _Iterations = iterations;
        _ExplosionForce = explosionForce;
        _ExplosionRadius = explosionRadius;
    }

    #endregion
}