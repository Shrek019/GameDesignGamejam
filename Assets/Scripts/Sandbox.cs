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

    [Header("Damage Settings")]
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
            // Vertraag de zombie
            enemy.ApplySlow(slowMultiplier, slowDuration);

            // Start een coroutine die zowel de sandbox als de zombie schade toebrengt
            StartCoroutine(DamageOverTime(enemy));
        }
    }

    private System.Collections.IEnumerator DamageOverTime(Zombie enemy)
    {
        while (enemy != null && currentHealth > 0)
        {
            // Sandbox neemt damage
            TakeDamage(damagePerEnemy);

            // Enemy neemt damage
            enemy.TakeDamage(damagePerEnemy);

            yield return new WaitForSeconds(damageInterval);
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
