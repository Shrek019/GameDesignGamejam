using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class Zombie : MonoBehaviour
{
    private Transform core;
    private WaveSpawner waveSpawner;

    [Header("Stats")]
    public int maxHealth = 20;
    private int currentHealth;

    [Header("Movement & Combat")]
    public float speed = 2f;
    public float attackInterval = 1.5f;
    public int damage = 5;
    public float attackRange = 1f;
    public float buildingDetectRange = 5f;

    private bool attacking = false;
    private Coroutine attackRoutine;
    private float baseSpeed;
    private Coroutine slowRoutine;
    private GameObject currentBuildingTarget;

    private Coroutine dotRoutine;

    private Rigidbody rb;

    public GameObject healthBarPrefab;
    private GameObject healthBarInstance;
    private Slider healthBarSlider;

    #region Initialization
    public void Init(Transform coreTransform, WaveSpawner spawner)
    {
        core = coreTransform;
        waveSpawner = spawner;
        baseSpeed = speed;
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // voorkomt vastlopen bij snelle collisions
        rb.isKinematic = false;

        // Spawn health bar
        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform.position + Vector3.up * 2f, Quaternion.identity, transform);
            healthBarSlider = healthBarInstance.GetComponentInChildren<Slider>();
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth;
        }
    }
    void Start()
    {
        // Start de DoT zodra de zombie spawnt
        dotRoutine = StartCoroutine(HealthDecay());
    }

    private IEnumerator HealthDecay()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f); // elke 2 seconden
            TakeDamage(1); // 1 health verliezen
        }
    }
    #endregion

    #region Physics Movement
    void FixedUpdate()
    {
        if (attacking) return;

        // Zoek dichtstbijzijnde building
        Collider[] hits = Physics.OverlapSphere(transform.position, buildingDetectRange);
        GameObject closestBuilding = null;
        float closestBuildingDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Building"))
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < closestBuildingDist)
                {
                    closestBuildingDist = dist;
                    closestBuilding = hit.gameObject;
                }
            }
        }

        float distToCore = Vector3.Distance(transform.position, core.position);

        if (closestBuilding != null && closestBuildingDist < distToCore)
            currentBuildingTarget = closestBuilding;
        else
            currentBuildingTarget = null;

        Vector3 targetPos = (currentBuildingTarget != null) ? currentBuildingTarget.transform.position : core.position;
        float distance = Vector3.Distance(transform.position, targetPos);

        if (distance > attackRange)
        {
            MoveTowards(targetPos);
        }
        else
        {
            if (currentBuildingTarget != null)
                StartAttackingBuilding(currentBuildingTarget);
            else
                StartAttackingCore();
        }
    }

    private void MoveTowards(Vector3 targetPos)
    {
        // Direction alleen op XZ plane
        Vector3 dir = (targetPos - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return; // niet bewegen bij minimale afstand

        dir.Normalize();

        // Rotation via Rigidbody
        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
        rb.MoveRotation(targetRot);

        // Beweging via Rigidbody
        Vector3 newPos = rb.position + dir * speed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);
    }
    #endregion

    #region Combat Core
    private void StartAttackingCore()
    {
        attacking = true;
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        attackRoutine = StartCoroutine(AttackCore());
    }

    private IEnumerator AttackCore()
    {
        while (true)
        {
            if (waveSpawner != null)
                waveSpawner.DamageCore(damage);
            yield return new WaitForSeconds(attackInterval);
        }
    }
    #endregion

    #region Combat Buildings
    private void StartAttackingBuilding(GameObject building)
    {
        attacking = true;
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        attackRoutine = StartCoroutine(AttackBuilding(building));
    }

    private IEnumerator AttackBuilding(GameObject building)
    {
        while (building != null)
        {
            BuildingManager buildingManager = building.GetComponent<BuildingManager>();
            if (buildingManager != null)
                buildingManager.TakeDamage(damage);
            else
                Destroy(building);

            yield return new WaitForSeconds(attackInterval);
        }

        attacking = false;
        currentBuildingTarget = null;
    }
    #endregion

    #region Slow Effect
    public void ApplySlow(float multiplier, float duration)
    {
        if (slowRoutine != null)
        {
            StopCoroutine(slowRoutine);
            speed = baseSpeed;
        }
        slowRoutine = StartCoroutine(SlowEffect(multiplier, duration));
    }

    private IEnumerator SlowEffect(float multiplier, float duration)
    {
        speed = baseSpeed * multiplier;
        yield return new WaitForSeconds(duration);
        speed = baseSpeed;
        slowRoutine = null;
    }
    #endregion

    #region Health
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (healthBarSlider != null)
            healthBarSlider.value = currentHealth;

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (waveSpawner != null)
            waveSpawner.ZombieDied();

        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        if (slowRoutine != null)
            StopCoroutine(slowRoutine);

        Destroy(gameObject);
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, buildingDetectRange);
    }
}
