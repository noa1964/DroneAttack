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
        // Find everything in the explosion radius
        Collider[] hits = Physics.OverlapSphere(pos, radius*12, affectedLayers);
        Debug.Log("Hit count: " + hits.Length);

        foreach (Collider hit in hits)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Optional: add upward lift for more realistic push
                //float upwardsModifier = 1f;

                Debug.Log($"force {force} pos {pos} radius: {radius} up: {upwardsModifier}");

                //rb.AddExplosionForce(
                //    10000,
                //    pos,
                //    100,
                //    upwardsModifier,
                //    ForceMode.Impulse
                //);

                Enemy enemy = hit.GetComponentInParent<Enemy>();
                if (enemy != null)
                {
                    Debug.Log("Dieeeeeeeeeeeeeeee " + enemy.gameObject.name);
                    enemy.gameObject.GetComponent<Animator>().enabled = false;
                    rb.AddExplosionForce(
                        10000,
                        pos,
                        100,
                        upwardsModifier,
                        ForceMode.Impulse
                    );

                    //enemy.Die();
                    StartCoroutine(PushEnemy(enemy));
                    //enemy.ApplyExplosionForce(400, pos, 50, upwardsModifier);
                }
                // Handle enemies
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

    private IEnumerator PushEnemy(Enemy enemy)
    {
        yield return new WaitForSeconds(0.5f);
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