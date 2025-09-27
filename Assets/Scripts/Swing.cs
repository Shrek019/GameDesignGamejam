using UnityEngine;

public class Swing : MonoBehaviour
{
    [Header("Swing Settings")]
    public float knockbackForce = 10f;
    public float range = 5f; // radius voor range preview
    public int damage = 3;

    void Update()
    {
        RotateTowardsEnemy();
    }

    void RotateTowardsEnemy()
    {
        Zombie target = GetClosestEnemy();
        if (target == null) return;

        Vector3 direction = (target.transform.position - transform.position);
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction, Vector3.up);

            // behoud je lokale X-rotatie
            Vector3 euler = targetRot.eulerAngles;
            euler.x = -90f; // je swing prefab X rotatie
            euler.z = 0f;   // optioneel, hou Z = 0
            targetRot = Quaternion.Euler(euler);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 5f * Time.deltaTime);
        }

        // Check range
        if (Vector3.Distance(transform.position, target.transform.position) <= range)
        {
            ApplyKnockback(target);
        }
    }

    void ApplyKnockback(Zombie zombie)
    {
        Rigidbody rb = zombie.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 dir = (zombie.transform.position - transform.position).normalized;
            rb.AddForce(dir * knockbackForce, ForceMode.Impulse);
        }

        zombie.TakeDamage(damage);
    }

    Zombie GetClosestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range);
        Zombie closest = null;
        float minDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            Zombie z = hit.GetComponent<Zombie>();
            if (z != null)
            {
                float dist = Vector3.Distance(transform.position, z.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = z;
                }
            }
        }
        return closest;
    }
}
