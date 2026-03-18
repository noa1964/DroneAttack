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
    [SerializeField] private float _HoleSize = 0.5f;
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
    [SerializeField] private float _SlowMotionDuration = 1;

    [Header("External Tools")]
    [SerializeField] private ProBuilderBooleanTool _ProBuilderTool;
    [SerializeField] private GameObject _SubMeshPrefab;
    [SerializeField] private float _MinScale = 2f;
    [SerializeField] private float _MaxScale = 7f;

    [Header("Impact Cameras")]
    [SerializeField] private Camera _TopCam;
    [SerializeField] private Camera _SideCam;
    [SerializeField] private Camera _ImpactCam;

    [SerializeField] private float _ImpactCamDistance = 1.5f;
    [SerializeField] private float _ImpactCamHeight = 0.3f;
    [SerializeField] private float _TopHeightOffset = 1.5f;
    [SerializeField] private float _SideDistance = 2.5f;
    [SerializeField] private float _SideHeight = 0.5f;

    [Header("Explosion")]
    [SerializeField] private BombExplosion _BombExplosion;
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

            float holeX = Mathf.Lerp(0.6f, 1.4f, _HoleSize);
            float holeY = Mathf.Lerp(0.45f, 1.1f, _HoleSize);

            //// clamp just in case
            //holeX = Mathf.Clamp(holeX, 0.2f, 2f);
            //holeY = Mathf.Clamp(holeY, 0.2f, 1.5f);

            spawnedSubMesh.transform.localScale =
                new Vector3(holeX, holeY, 2f);
            spawnedSubMesh.transform.position = hitPoint;

            (_currentRemaning, _currentHole) =
                _ProBuilderTool.CutHoleWithResult(wall, spawnedSubMesh);

            SpawnImpactParticles(hitPoint, hitNormal);

            PositionImpactCameras(hitPoint, hitNormal);

            if (_currentHole != null)
            {
                ShatterRecursive(_currentHole, hitPoint, hitNormal, _Iterations);
            }

            _BombExplosion.Explode(_ExplosionRadius, _ExplosionForce, hitPoint);
            Quaternion volumeRotation = Quaternion.LookRotation(-hitNormal, Vector3.up);

            _currentVolume = Instantiate(
                _VolumeEffectParticleSys,
                hitPoint,
                volumeRotation
            ).gameObject;

            float scaleValue = Mathf.Lerp(_MinVolume, _MaxVolume, _HoleSize);
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

    private void PositionImpactCameras(Vector3 hitPoint, Vector3 hitNormal)
    {
        // Same rotation calculation you already use
        float yRotation = Mathf.Atan2(hitNormal.x, hitNormal.z) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.Euler(90f, yRotation, 0f);

        // -------- TOP CAMERA --------
        if (_TopCam != null)
        {
            Vector3 topPos = hitPoint + Vector3.up * _TopHeightOffset;

            _TopCam.transform.position = topPos;
            _TopCam.transform.rotation = rot;
        }

        // -------- SIDE CAMERA --------
        if (_SideCam != null)
        {
            Vector3 sidePos = hitPoint;
            if (Mathf.Abs(hitNormal.x) > Mathf.Abs(hitNormal.z))
            {
                sidePos += Vector3.forward * _SideDistance;
            }
            else
            {
                sidePos += Vector3.right * _SideDistance;
            }

            sidePos.y = hitPoint.y;
            _SideCam.transform.position = sidePos;
            _SideCam.transform.LookAt(hitPoint);
        }

        // -------- ENEMIES CAMERA --------
        if (_ImpactCam != null)
        {

            Vector3 explosionDir = -hitNormal;

            // Position camera slightly BACK from explosion
            Vector3 camPos = hitPoint
                - explosionDir * _ImpactCamDistance   // move backward
                + Vector3.up * _ImpactCamHeight;      // slight height

            _ImpactCam.transform.position = camPos;

            // Look toward explosion direction
            _ImpactCam.transform.rotation = Quaternion.LookRotation(explosionDir, Vector3.up);
        }
    }

    private IEnumerator SlowMotionRoutine()
    {
        // Lerp to slow motion
        TimeManager.Instance.LerpTimeScale(_SlowMotionScale, 0.1f);

        yield return new WaitForSecondsRealtime(_SlowMotionDuration);

        // Lerp back to normal if not paused
        if (!TimeManager.Instance.IsPaused)
            TimeManager.Instance.LerpTimeScale(1f, 0.2f);
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

        Quaternion rotation = Quaternion.Euler(90f, yRotation, 0f);

        if (_ExplodeParticleSys != null)
        {
            //ParticleSystem explode = Instantiate(_ExplodeParticleSys, position, rot);
            _ExplodeParticleSys.transform.position = position;
            _ExplodeParticleSys.transform.rotation = rotation;
            _ExplodeParticleSys.Play();
            Destroy(_ExplodeParticleSys.gameObject, _ExplodeParticleSys.main.duration);
        }

        if (_StonesParticleSys != null)
        {
            //ParticleSystem stones = Instantiate(_StonesParticleSys, position, rot);
            _StonesParticleSys.transform.position = position;
            _StonesParticleSys.transform.rotation = rotation;
            _StonesParticleSys.Play();
            Destroy(_StonesParticleSys.gameObject, _StonesParticleSys.main.duration);
        }
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

    public void SetSettingsValues(
        float hole,
        int iterations,
        float explosionForce,
        float explosionRadius
    )
    {
        _HoleSize = hole;
        _Iterations = iterations;
        _ExplosionForce = explosionForce;
        _ExplosionRadius = explosionRadius;
    }

    public void SetIsSlowMotion(bool value)
    {
        Debug.Log("set slow " + value);
        _UseSlowMotion = value;
    }

    //public void SetSlowMotionValue(float value)
    //{
    //    Debug.Log("set slow " + value);
    //    _SlowMotionScale = value;

    //}



}