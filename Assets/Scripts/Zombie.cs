using UnityEngine;

public class Zombie : MonoBehaviour
{
    private Transform target;
    private WaveSpawner waveSpawner;
    public float speed = 2f;
    public int damage = 5;
    public float attackInterval = 1.5f;
    private bool attacking = false;

    // Deze methode wordt vanuit WaveSpawner aangeroepen
    public void Init(Transform core, WaveSpawner spawner)
    {
        target = core;
        waveSpawner = spawner;
    }

    void Update()
    {
        if (target != null && !attacking)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target.position) < 0.5f)
            {
                StartCoroutine(AttackCore());
            }
        }
    }

    System.Collections.IEnumerator AttackCore()
    {
        attacking = true;
        while (true)
        {
            waveSpawner.DamageCore(damage);
            yield return new WaitForSeconds(attackInterval);
        }
    }

    private void OnDestroy()
    {
        if (waveSpawner != null)
            waveSpawner.ZombieDied();
    }
}
