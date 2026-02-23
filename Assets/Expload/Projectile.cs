using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private WallShatter wallShatter;

    private Rigidbody rb;
    private Vector3 originalPos;
    private Quaternion originalRot;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        originalPos = transform.position;
        originalRot = transform.rotation;
    }

    private void FixedUpdate()
    {
        rb.velocity = transform.forward * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            ContactPoint contact = collision.contacts[0];

            Vector3 hitPoint = contact.point;
            Vector3 hitNormal = contact.normal;

            wallShatter.ShatterWall(collision.gameObject, hitPoint, hitNormal);
        }
    }

    public void ResetProjectile()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = originalPos;
        transform.rotation = originalRot;
    }
}