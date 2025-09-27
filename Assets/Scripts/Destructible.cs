using UnityEngine;

public class Destructible : MonoBehaviour
{
    public int health = 20;

    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
