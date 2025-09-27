using UnityEngine;

public class Sandbox : MonoBehaviour
{
    [Header("Slow Settings")]
    [Range(0.1f, 1f)]
    public float slowMultiplier = 0.5f; // halve snelheid
    public float slowDuration = 1f;

    [Header("Durability Settings")]
    public int maxHealth = 50;
    private int currentHealth;

    public int damagePerEnemy = 1;       // hoeveel schade per enemy
    public float damageInterval = 1f;    // hoe vaak enemies schade toebrengen (per seconde)

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void OnTriggerEnter(Collider other)
    {
        Zombie enemy = other.GetComponent<Zombie>();
        if (enemy != null)
        {
            enemy.ApplySlow(slowMultiplier, slowDuration);
            StartCoroutine(DamageOverTime());
        }
    }

    private System.Collections.IEnumerator DamageOverTime()
    {
        while (true)
        {
            TakeDamage(damagePerEnemy);
            yield return new WaitForSeconds(damageInterval);

            if (currentHealth <= 0)
                yield break; // stopt als kapot
        }
    }

    private void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Destroy(gameObject); // Sandbox kapot
        }
    }
}
