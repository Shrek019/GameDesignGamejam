using UnityEngine;

public class Slide : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public Transform projectileSpawn;
    public float shootInterval = 2f;
    public float projectileSpeed = 15f;
    public int damage = 10;

    [Header("Detection Settings")]
    public float range = 5f; // detection range for preview
    public float rotationSpeed = 5f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        Zombie target = GetClosestEnemy();
        if (target != null)
        {
            // Draai naar de vijand
            Vector3 direction = (target.transform.position - transform.position).normalized;
            direction.y = 0f;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            }

            if (timer >= shootInterval)
            {
                Shoot(target);
                timer = 0f;
            }
        }
    }

    void Shoot(Zombie target)
    {
        if (projectilePrefab == null || projectileSpawn == null) return;

        Vector3 dir = (target.transform.position - projectileSpawn.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, projectileSpawn.position, Quaternion.LookRotation(dir));
        Projectile p = proj.GetComponent<Projectile>();
        if (p != null) p.damage = damage;

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = dir * projectileSpeed;

        Destroy(proj, 5f);
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
