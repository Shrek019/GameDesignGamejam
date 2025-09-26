using UnityEngine;

public class Slide : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform projectileSpawn;
    public float shootInterval = 2f;
    public float projectileSpeed = 15f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= shootInterval)
        {
            Shoot();
            timer = 0f;
        }
    }

    void Shoot()
    {
        GameObject proj = Instantiate(projectilePrefab, projectileSpawn.position, projectileSpawn.rotation);
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * projectileSpeed;
        }
        Destroy(proj, 5f); // auto destroy na 5 sec
    }
}
