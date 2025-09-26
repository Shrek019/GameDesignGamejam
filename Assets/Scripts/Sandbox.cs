using UnityEngine;

public class Sandbox : MonoBehaviour
{
    [Range(0.1f, 1f)]
    public float slowMultiplier = 0.5f; // halve snelheid
    public float slowDuration = 1f;

    private void OnTriggerEnter(Collider other)
    {
        Zombie enemy = other.GetComponent<Zombie>();
        if (enemy != null)
        {
            enemy.ApplySlow(slowMultiplier, slowDuration);
        }
    }
}
