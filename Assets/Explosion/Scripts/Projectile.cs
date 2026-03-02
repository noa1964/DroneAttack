using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    //[SerializeField] private WallShatter wallShatter;

    private Rigidbody rb;

    private Vector3 originalPos;
    private Quaternion originalRot;

    private bool isDestroy = false;

    private Vector3 targetPoint;
    private Vector3 moveDirection; // final direction to move after reaching target
    private bool reachedTarget = false;
    private float stopThreshold = 0.1f; // distance to consider "reached"

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalPos = transform.position;
        originalRot = transform.rotation;
    }

    private void FixedUpdate()
    {
        if (targetPoint != Vector3.zero)
        {
            if (!reachedTarget)
            {
                Vector3 dir = (targetPoint - transform.position);
                if (dir.magnitude <= stopThreshold)
                {
                    // Reached target, lock direction
                    reachedTarget = true;
                    moveDirection = dir.normalized; // still small movement direction
                }
                else
                {
                    moveDirection = dir.normalized;
                }
            }

            rb.velocity = moveDirection * speed;

            // Optional: rotate to face movement
            if (rb.velocity.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(rb.velocity);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall") && !isDestroy)
        {
            isDestroy = true;

            ContactPoint contact = collision.contacts[0];
            Vector3 hitPoint = contact.point;
            Vector3 hitNormal = contact.normal;

            Debug.Log("collide");
            WallShatter.Instance.ShatterWall(
                collision.gameObject,
                hitPoint,
                hitNormal
            );

            StartCoroutine(DeactivateAfterDelay(0.5f));
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
        {
            rb = GetComponent<Rigidbody>();
        }

        isDestroy = false;
        reachedTarget = false;
        targetPoint = target;

        moveDirection = Vector3.zero;

        gameObject.SetActive(true);

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // transform.position = originalPos;
        // transform.rotation = originalRot;
    }
}