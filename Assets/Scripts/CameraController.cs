using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float moveSpeed = 10f;
    public float verticalSpeed = 5f;
    public float zoomSpeed = 200f;
    public float rotationSpeed = 3f;

    [Header("Movement Bounds")]
    public Vector2 xBounds = new Vector2(-50, 50);
    public Vector2 yBounds = new Vector2(2, 50);
    public Vector2 zBounds = new Vector2(-50, 50);

    [Header("Zoom Bounds")]
    public float minFOV = 20f;
    public float maxFOV = 80f;

    private Camera cam;
    private float rotationX = 0f;
    private float rotationY = 0f;

    void Start()
    {
        cam = GetComponent<Camera>();

        Vector3 angles = transform.eulerAngles;
        rotationX = angles.y;
        rotationY = angles.x;
    }

    void Update()
    {
        HandleMovement();
        HandleZoom();
        HandleRotation();
    }

    void HandleMovement()
    {
        float moveX = 0f;
        float moveZ = 0f;
        float moveY = 0f;

        // Arrow keys → Forward/Back & Left/Right
        if (Input.GetKey(KeyCode.UpArrow))
            moveZ += 1;

        if (Input.GetKey(KeyCode.DownArrow))
            moveZ -= 1;

        if (Input.GetKey(KeyCode.RightArrow))
            moveX += 1;

        if (Input.GetKey(KeyCode.LeftArrow))
            moveX -= 1;

        // Q / E → Up / Down
        if (Input.GetKey(KeyCode.E))
            moveY += 1;

        if (Input.GetKey(KeyCode.Q))
            moveY -= 1;

        Vector3 move = transform.forward * moveZ +
                       transform.right * moveX +
                       Vector3.up * moveY;

        transform.position += move * moveSpeed * Time.deltaTime;

        // Apply Bounds
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, xBounds.x, xBounds.y);
        pos.y = Mathf.Clamp(pos.y, yBounds.x, yBounds.y);
        pos.z = Mathf.Clamp(pos.z, zBounds.x, zBounds.y);
        transform.position = pos;
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            cam.fieldOfView -= scroll * zoomSpeed * Time.deltaTime;
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minFOV, maxFOV);
        }
    }

    void HandleRotation()
    {
        if (Input.GetMouseButton(1)) // Right mouse drag
        {
            rotationX += Input.GetAxis("Mouse X") * rotationSpeed * 100f * Time.deltaTime;
            rotationY -= Input.GetAxis("Mouse Y") * rotationSpeed * 100f * Time.deltaTime;

            rotationY = Mathf.Clamp(rotationY, -80f, 80f);

            transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);
        }
    }
}