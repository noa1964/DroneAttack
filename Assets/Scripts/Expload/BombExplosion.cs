using System.Collections;
using UnityEngine;

public class BombExplosion : MonoBehaviour
{
    public float _Radius = 5f;
    public float _Force = 800f;
    public float upwardsModifier = 1.5f;

    public LayerMask affectedLayers;

    public void Explode(float radius, float force, Vector3 pos)
    {
        // Find everything in the explosion radius
        Collider[] hits = Physics.OverlapSphere(pos, radius, affectedLayers);
        Debug.Log("Hit count: " + hits.Length);

        foreach (Collider hit in hits)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Optional: add upward lift for more realistic push
                float upwardsModifier = 1f;

                Debug.Log($"force {force} pos {pos} radius: {radius} up: {upwardsModifier}");
                rb.AddExplosionForce(
                    force,
                    pos,
                    radius,
                    upwardsModifier,
                    ForceMode.Impulse
                );

                // Handle enemies
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.Die();
                    //StartCoroutine(PushEnemy(rb));
                }
            }
        }
    }

    private IEnumerator PushEnemy(Rigidbody rb)
    {
        yield return new WaitForSeconds(1f);
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

    }
    // Optional: explode automatically
    void Start()
    {
        //Explode();
    }

    // Draw radius in scene
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _Radius);
    }
}