using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class Zombie : MonoBehaviour
{
    private Transform target;
    private WaveSpawner waveSpawner;
    private NavMeshAgent agent;

    [Header("Zombie Settings")]
    public float speed = 2f;
    public int damage = 5;
    public float attackInterval = 1.5f;

    private bool attacking = false;
    private Coroutine attackRoutine;

    private float baseSpeed;
    private Coroutine slowRoutine;

    // Deze methode wordt vanuit WaveSpawner aangeroepen
    public void Init(Transform core, WaveSpawner spawner)
    {
        target = core;
        waveSpawner = spawner;

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        baseSpeed = speed;
        agent.speed = baseSpeed;
        agent.stoppingDistance = 0.5f; // wanneer dichtbij core stoppen
        agent.SetDestination(target.position);
    }

    void Update()
    {
        if (agent == null || target == null) return;

        if (!attacking && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            StartAttacking();
        }
    }

    private void StartAttacking()
    {
        attacking = true;
        agent.isStopped = true;

        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

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

    public void ApplySlow(float multiplier, float duration)
    {
        if (slowRoutine != null)
        {
            StopCoroutine(slowRoutine);
            agent.speed = baseSpeed; // reset vorige slow
        }

        slowRoutine = StartCoroutine(SlowEffect(multiplier, duration));
    }

    private IEnumerator SlowEffect(float multiplier, float duration)
    {
        agent.speed = baseSpeed * multiplier;
        yield return new WaitForSeconds(duration);
        agent.speed = baseSpeed;
        slowRoutine = null;
    }

    private void OnDestroy()
    {
        if (waveSpawner != null)
            waveSpawner.ZombieDied();

        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        if (slowRoutine != null)
            StopCoroutine(slowRoutine);
    }
}
