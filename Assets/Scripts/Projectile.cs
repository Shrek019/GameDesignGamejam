using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 5;

    private void OnTriggerEnter(Collider other)
    {
        Zombie zombie = other.GetComponent<Zombie>();
        Debug.Log(zombie);
        if (zombie != null)
        {
            zombie.TakeDamage(damage);
            Destroy(gameObject); // Projectile verdwijnt na hit
        }
    }
}
