using System.Collections;
using UnityEngine;

public class BombExplosion : MonoBehaviour
{
    public float _Radius = 5f;
    public float _Force = 800f;
    public float upwardsModifier = 1.5f;

    public LayerMask affectedLayers;
    public Rigidbody rigi;

    public void Explode(float radius, float force, Vector3 pos)
    {
        radius *= 10;
        Collider[] hits = Physics.OverlapSphere(pos, radius, affectedLayers);

        foreach (Collider hit in hits)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb == null) continue;

            Enemy enemy = hit.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                enemy.gameObject.GetComponent<Animator>().enabled = false;
                StartCoroutine(PushEnemy(enemy, rb, pos, force, radius));
            }
        }
    }

    public void Explode(Transform trans)
    {
        rigi.AddExplosionForce(400,
                    trans.position,
                    60,
                    upwardsModifier,
                    ForceMode.Impulse);
    }

    private IEnumerator PushEnemy(Enemy enemy, Rigidbody rb, Vector3 pos, float force, float radius)
    {
        yield return new WaitForSeconds(0.1f);

        rb.AddExplosionForce(
            force,
            pos,
            radius,
            upwardsModifier,
            ForceMode.Impulse
        );

        yield return new WaitForSeconds(0.05f);
        enemy.Die();
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