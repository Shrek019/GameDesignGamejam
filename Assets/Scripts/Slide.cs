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
    public float detectionRange = 5f;
    public float rotationSpeed = 5f;
    [Range(0f, 1f)] public float forwardThreshold = 0.7f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        Zombie target = GetClosestEnemy();
        if (target != null)
        {
            // Draai naar de vijand
            Vector3 direction = (target.transform.position - transform.position).normalized;
            direction.y = 0; // enkel horizontaal draaien
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(-direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            }

            // Schiet interval check
            if (timer >= shootInterval)
            {
                Shoot(direction);
                timer = 0f;
            }
        }
    }

    Zombie GetClosestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);
        float closestDist = Mathf.Infinity;
        Zombie closest = null;

        foreach (var hit in hits)
        {
            Zombie enemy = hit.GetComponent<Zombie>();
            if (enemy != null)
            {
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = enemy;
                }
            }
        }
        return closest;
    }

    bool EnemyBehind()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);
        foreach (var hit in hits)
        {
            Zombie enemy = hit.GetComponent<Zombie>();
            if (enemy != null)
            {
                // Richting naar enemy
                Vector3 dirToEnemy = (enemy.transform.position - transform.position).normalized;

                // Check of enemy in "achter-cone" zit
                float dot = Vector3.Dot(-transform.forward, dirToEnemy);
                if (dot >= forwardThreshold)
                {
                    return true;
                }
            }
        }
        return false;
    }

    void Shoot(Vector3 direction)
    {
        if (projectilePrefab == null || projectileSpawn == null) return;

        GameObject proj = Instantiate(projectilePrefab, projectileSpawn.position, Quaternion.LookRotation(direction));
        Projectile p = proj.GetComponent<Projectile>();
        if (p != null) p.damage = damage;

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = direction * projectileSpeed;

        Destroy(proj, 5f);
    }

    //private void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.cyan;
    //    Gizmos.DrawWireSphere(transform.position, detectionRange);
    //}

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Cone voor achterzijde
        Vector3 right = Quaternion.Euler(0, Mathf.Acos(forwardThreshold) * Mathf.Rad2Deg, 0) * -transform.forward;
        Vector3 left = Quaternion.Euler(0, -Mathf.Acos(forwardThreshold) * Mathf.Rad2Deg, 0) * -transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + right * detectionRange);
        Gizmos.DrawLine(transform.position, transform.position + left * detectionRange);
    }
}
