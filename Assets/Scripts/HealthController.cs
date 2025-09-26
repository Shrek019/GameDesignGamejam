using UnityEngine;
using UnityEngine.UI; // voor UI Slider

public class HealthController : MonoBehaviour
{
    [Header("Health Settings")]
    public Slider healthBar;
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Rain Settings")]
    public RainController rainController; // Sleep hier je RainController in
    public float healthDecreaseRate = 5f; // per seconde als het regent

    void Start()
    {
        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    void Update()
    {
        if (rainController != null && rainController.IsRaining()) // check of het regent
        {
            currentHealth -= healthDecreaseRate * Time.deltaTime;
            currentHealth = Mathf.Max(currentHealth, 0f); // niet onder 0

            if (healthBar != null)
                healthBar.value = currentHealth;
        }
    }
}
