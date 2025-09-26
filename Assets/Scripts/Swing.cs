using UnityEngine;

public class Swing : MonoBehaviour
{
    public float knockbackForce = 10f;

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && other.CompareTag("Enemy"))
        {
            Vector3 direction = (other.transform.position - transform.position).normalized;
            rb.AddForce(direction * knockbackForce, ForceMode.Impulse);
        }
    }
}
