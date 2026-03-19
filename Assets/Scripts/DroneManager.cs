using UnityEngine;

public class DroneManager : MonoBehaviour
{
    public static DroneManager Instance;

    [Header("Drone")]
    [SerializeField] private Projectile dronePrefab;
    [SerializeField] private Transform droneSpawnPoint;

    [Header("Target Indicator")]
    [SerializeField] private GameObject targetIndicatorPrefab;

    [Header("UI")]
    [SerializeField] private GameObject _UICameras;

    private GameObject currentTargetIndicator;

    private Vector3 targetPoint;
    private bool hasTarget = false;

    private void Awake()
    {
        Instance = this;
    }

    // Called by Building when user clicks
    public void SetTargetPoint(Vector3 point)
    {
        targetPoint = point;
        hasTarget = true;

        Debug.Log("Drone target updated: " + targetPoint);

        // If indicator doesn't exist → create it
        if (currentTargetIndicator == null)
        {
            currentTargetIndicator = Instantiate(targetIndicatorPrefab, targetPoint, Quaternion.identity);
        }
        else
        {
            // Otherwise just move it
            currentTargetIndicator.transform.position = targetPoint;
        }
    }

    // Call this from a button
    public void SpawnDrone()
    {
        if (!hasTarget)
        {
            Debug.LogWarning("No target selected!");
            return;
        }

        _UICameras.SetActive(true);
        UIManager.Instance.SetEnemyPanelActive(false);
        // Spawn the drone
        Projectile drone = Instantiate(
            dronePrefab,
            droneSpawnPoint.position,
            Quaternion.identity
        );

        drone.ResetProjectile(targetPoint);

        // Destroy the target indicator
        if (currentTargetIndicator != null)
        {
            Destroy(currentTargetIndicator);
            currentTargetIndicator = null;
        }

        // Clear target state
        hasTarget = false;
    }
}