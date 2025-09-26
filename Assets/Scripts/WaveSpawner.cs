using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    [Header("Zombie Settings")]
    public GameObject zombiePrefab;
    public Transform[] spawnPoints; // meerdere spawnpoints
    public Transform core;

    [Header("Wave Settings")]
    public int waveNumber = 1;
    private bool waveActive = false;

    [Header("Core Health")]
    public Slider coreHealthSlider;
    public int coreMaxHealth = 100;
    private int coreCurrentHealth;

    void Start()
    {
        coreCurrentHealth = coreMaxHealth;
        coreHealthSlider.maxValue = coreMaxHealth;
        coreHealthSlider.value = coreMaxHealth;
    }

    void Update()
    {
        // Spatie = start wave
        if (Input.GetKeyDown(KeyCode.Space) && !waveActive)
        {
            StartCoroutine(SpawnWave());
        }
    }

    IEnumerator SpawnWave()
    {
        waveActive = true;

        // aantal zombies = 4 + (waveNumber - 1) * 2  t.e.m. 7 + (waveNumber - 1) * 2
        int zombieCount = Random.Range(4 + (waveNumber - 1) * 2, 8 + (waveNumber - 1) * 2);

        for (int i = 0; i < zombieCount; i++)
        {
            // kies random spawnpoint
            Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

            GameObject z = Instantiate(zombiePrefab, spawn.position, Quaternion.identity);
            z.GetComponent<Zombie>().Init(core, this);

            yield return new WaitForSeconds(0.5f); // kleine delay tussen zombies
        }
    }

    public void ZombieDied()
    {
        // check of er nog zombies in de scene zijn
        if (GameObject.FindObjectsByType<Zombie>(FindObjectsSortMode.None).Length <= 1)
        {
            waveActive = false;
            waveNumber++;
        }
    }

    public void DamageCore(int damage)
    {
        coreCurrentHealth -= damage;
        coreHealthSlider.value = coreCurrentHealth;

        if (coreCurrentHealth <= 0)
        {
            Debug.Log("Game Over! De speeltuin is gevallen!");
            // hier kan je game over scherm triggeren
        }
    }
}
