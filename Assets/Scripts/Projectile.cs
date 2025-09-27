using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 5;

    private void OnTriggerEnter(Collider other)
    {
        Zombie zombie = other.GetComponent<Zombie>();
        if (zombie != null)
        {
            zombie.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
