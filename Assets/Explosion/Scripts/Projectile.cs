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

    private Vector3 targetPoint;
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
        // Stop movement if destroyed or no target
        if (isDestroy || !hasTarget)
            return;

        if (!reachedTarget)
        {
            Vector3 dir = targetPoint - transform.position;

            if (dir.magnitude <= stopThreshold)
            {
                reachedTarget = true;
                moveDirection = dir.normalized;
            }
            else
            {
                moveDirection = dir.normalized;
            }
        }

        rb.velocity = moveDirection * speed;

        // Rotate projectile toward movement
        if (rb.velocity.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(rb.velocity);
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

            StartCoroutine(DeactivateAfterDelay(0.2f));
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