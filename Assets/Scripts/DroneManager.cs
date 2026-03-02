using UnityEngine;

public class DroneManager : MonoBehaviour
{
    public static DroneManager Instance;

    [Header("Drone")]
    [SerializeField] private Projectile dronePrefab;
    [SerializeField] private Transform droneSpawnPoint;

    public Vector3 targetPoint;
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
    }

    // Call this from a button
    public void SpawnDrone()
    {
        if (!hasTarget)
        {
            Debug.LogWarning("No target selected!");
            return;
        }

        Projectile drone = Instantiate(
            dronePrefab,
            droneSpawnPoint.position,
            Quaternion.identity
        );

        drone.ResetProjectile(targetPoint);
    }
}