using UnityEngine;

public class Swing : MonoBehaviour
{
    public float knockbackForce = 10f;
    public float knockbackForceRange = 5f; // radius for preview
    public int damage = 3;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb != null)
            {
                // Knockback
                Vector3 direction = transform.forward;
                rb.AddForce(direction * knockbackForce, ForceMode.Impulse);
            }

            // Damage zombie
            Zombie zombie = other.GetComponent<Zombie>();
            if (zombie != null)
            {
                zombie.TakeDamage(damage);
                Debug.Log($"Swing hit {zombie.name}, dealt {damage} damage.");
            }
        }
    }
}
