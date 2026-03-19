using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;

    private Rigidbody rb;

    private Vector3 originalPos;
    private Quaternion originalRot;

    private bool isDestroy = false;

    private Vector3 targetPoint = new Vector3(58f, 7f, -11f);
    private Vector3 moveDirection;
    private bool reachedTarget = false;
    private bool hasTarget = false;

    private float stopThreshold = 0.1f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalPos = transform.position;
        originalRot = transform.rotation;
    }

    private void FixedUpdate()
    {
        if (isDestroy || !hasTarget)
            return;

        if (!reachedTarget)
        {
            Vector3 dir = targetPoint - transform.position;
            float distance = dir.magnitude;

            // Check if we've reached target
            if (distance <= stopThreshold)
            {
                reachedTarget = true;
                rb.velocity = Vector3.zero;
                return;
            }

            moveDirection = dir.normalized;

            // --- Smooth slowing ---
            // Slow down when within 3 units of target
            float slowDistance = 3f;
            float speedMultiplier = 1f;

            if (distance < slowDistance)
            {
                // Use a smoother curve: squared
                float t = Mathf.Clamp01(distance / slowDistance);
                speedMultiplier = Mathf.Sqrt(t); // gentler slow down
            }

            rb.velocity = moveDirection * speed * speedMultiplier;

            // --- Lean forward ---
            // Tilt the projectile along the movement direction
            if (rb.velocity.sqrMagnitude > 0.01f)
            {
                // --- Lean forward in the opposite direction ---
                Quaternion targetRotation = Quaternion.LookRotation(rb.velocity);

                // Optional: tilt slightly backward along X if you want "opposite lean"
                float leanAngle = Mathf.Lerp(0, 30f, 1 - speedMultiplier); // adjust max angle
                targetRotation *= Quaternion.Euler(-leanAngle, 0, 0); // negative = backward

                // Smoothly rotate toward velocity
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall") && !isDestroy)
        {
            isDestroy = true;

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;

            GetComponent<Collider>().enabled = false;

            ContactPoint contact = collision.contacts[0];
            Vector3 hitPoint = contact.point;
            Vector3 hitNormal = contact.normal;

            Debug.Log("collide");

            WallShatter.Instance.ShatterWall(
                collision.gameObject,
                hitPoint,
                hitNormal
            );

            StartCoroutine(DeactivateAfterDelay(0.1f));
        }
    }

    private IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    public void ResetProjectile(Vector3 target)
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        isDestroy = false;
        reachedTarget = false;
        hasTarget = true;

        targetPoint = target;
        moveDirection = Vector3.zero;

        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        GetComponent<Collider>().enabled = true;

        // Optional reset transform
        // transform.position = originalPos;
        // transform.rotation = originalRot;

        gameObject.SetActive(true);
    }
}