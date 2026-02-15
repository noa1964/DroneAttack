using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraDragHandler : MonoBehaviour, IDragHandler, IScrollHandler
{
    [Header("References")]
    public Camera targetCamera;
    public Transform target; // The point to orbit around

    [Header("Rotation Settings")]
    public float rotationSpeed = 0.2f;
    public float minVerticalAngle = 10f;
    public float maxVerticalAngle = 80f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 0.5f;
    public float minDistance = 1f;
    public float maxDistance = 7f;

    private float _distance;
    private float _yaw;
    private float _pitch;

    private void Start()
    {
        if (targetCamera == null) return;

        Vector3 angles = targetCamera.transform.eulerAngles;
        _yaw = angles.y;
        _pitch = angles.x;

        //_distance = Vector3.Distance(targetCamera.transform.position, target.position);
    }

    public void SetParent(Transform transform)
    {
        target = transform;
        _distance = Vector3.Distance(targetCamera.transform.position, target.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (targetCamera == null || target == null) return;

        Vector2 delta = eventData.delta;

        _yaw += delta.x * rotationSpeed;

        UpdateCameraPosition();
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (targetCamera == null || target == null) return;

        _distance -= eventData.scrollDelta.y * zoomSpeed;
        _distance = Mathf.Clamp(_distance, minDistance, maxDistance);

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        // Compute horizontal rotation around Y
        Quaternion rotation = Quaternion.Euler(0f, _yaw, 0f);
        Vector3 direction = rotation * Vector3.back * _distance;

        // Compute new position
        Vector3 newPos = target.position + direction;

        // Keep original Y height
        newPos.y = targetCamera.transform.position.y;

        targetCamera.transform.position = newPos;

        // Compute rotation manually: only rotate around Y, X stays 0
        Vector3 lookDir = target.position - newPos;
        lookDir.y = 0; // ignore vertical difference
        if (lookDir.sqrMagnitude > 0.001f) // avoid zero-length
        {
            targetCamera.transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        }
    }
}
