using System.Collections;
using UnityEngine;

public class BombExplosion : MonoBehaviour
{
    public float _Radius = 5f;
    public float _Force = 800f;
    public float upwardsModifier = 1.5f;

    public LayerMask affectedLayers;
    public Rigidbody rigi;

    [Header("Gizmo Debug")]
    public bool showGizmosAlways = true;
    public bool showLastExplosion = true;
    public Color gizmoColor = new Color(1f, 0f, 0f, 0.6f);

    // tracked last explosion (for visualization)
    private Vector3 lastExplosionPos;
    private float lastExplosionRadius = -1f;

    public void Explode(float radius, float force, Vector3 pos)
    {
        radius += 2;
        // store for debug visualization
        lastExplosionPos = pos;
        lastExplosionRadius = radius;

        //radius *= 10;
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
        // record explosion for visualization (this overload uses fixed values)
        lastExplosionPos = trans.position;
        lastExplosionRadius = 60f;

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

    // Draw radius in scene while selected and/or always (use inspector toggles)
    void OnDrawGizmos()
    {
        if (showGizmosAlways)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, _Radius);
        }

        if (showLastExplosion && lastExplosionRadius > 0f)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(lastExplosionPos, lastExplosionRadius);
        }
    }

    void OnDrawGizmosSelected()
    {
        // keep a distinct color when selected
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _Radius);
    }
}