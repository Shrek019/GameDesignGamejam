using UnityEngine;

public class Slide : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public Transform projectileSpawn;
    public float shootInterval = 2f;
    public float projectileSpeed = 15f;
    public int damage = 10; // hoeveel schade elk projectile doet

    [Header("Detection Settings")]
    public float detectionRange = 3f; // radius van detectie
    [Range(0f, 1f)] public float forwardThreshold = 0.7f; // 1 = exact vooruit, 0.7 ≈ 45° cone

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= shootInterval && EnemyInFront())
        {
            Shoot();
            timer = 0f;
        }
    }

    bool EnemyInFront()
    {
        // Alle colliders in range
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);
        foreach (var hit in hits)
        {
            Zombie enemy = hit.GetComponent<Zombie>();
            if (enemy != null)
            {
                // Richting naar enemy
                Vector3 dirToEnemy = (enemy.transform.position - transform.position).normalized;

                // Check of enemy in front cone staat
                float dot = Vector3.Dot(transform.forward, dirToEnemy);
                if (dot >= forwardThreshold)
                {
                    return true;
                }
            }
        }
        return false;
    }

    void Shoot()
    {
        GameObject proj = Instantiate(projectilePrefab, projectileSpawn.position, projectileSpawn.rotation);

        // Voeg damage toe aan projectile
        Projectile p = proj.GetComponent<Projectile>();
        if (p != null) p.damage = damage;

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = transform.forward * projectileSpeed;

        Destroy(proj, 5f); // auto destroy
    }

    // Range visualisatie in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Cone-indicatie (optioneel)
        Vector3 right = Quaternion.Euler(0, Mathf.Acos(forwardThreshold) * Mathf.Rad2Deg, 0) * transform.forward;
        Vector3 left = Quaternion.Euler(0, -Mathf.Acos(forwardThreshold) * Mathf.Rad2Deg, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + right * detectionRange);
        Gizmos.DrawLine(transform.position, transform.position + left * detectionRange);
    }
}
