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
    public float detectionRange = 3f;
    [Range(0f, 1f)] public float forwardThreshold = 0.7f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= shootInterval && EnemyBehind())
        {
            Shoot();
            timer = 0f;
        }
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

    void Shoot()
    {
        Vector3 spawnPos = new Vector3(projectileSpawn.position.x, projectileSpawn.position.y / 2, projectileSpawn.position.z);
        GameObject proj = Instantiate(projectilePrefab, spawnPos, projectileSpawn.rotation);

        Projectile p = proj.GetComponent<Projectile>();
        if (p != null) p.damage = damage;

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = -transform.forward * projectileSpeed; // schiet naar achteren

        Destroy(proj, 5f);
    }

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
