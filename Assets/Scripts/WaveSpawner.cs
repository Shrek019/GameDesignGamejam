using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class WaveSpawner : MonoBehaviour
{
    [Header("Zombie Settings")]
    public GameObject zombiePrefab;
    public Transform[] spawnPoints; // 4 pijlen: boven, onder, links, rechts
    public GameObject[] arrowIndicators; // verwijzingen naar de pijlen/indicatoren
    public Transform core;

    [Header("Wave Settings")]
    public int waveNumber = 1;
    public int maxWaves = 5;
    private bool waveActive = false;
    private int zombiesAlive;

    private List<int> activeSpawnIndices = new List<int>();

    [Header("Core Health")]
    public Slider coreHealthSlider;
    public int coreMaxHealth = 100;
    private int coreCurrentHealth;

    void Start()
    {
        coreCurrentHealth = coreMaxHealth;
        coreHealthSlider.maxValue = coreMaxHealth;
        coreHealthSlider.value = coreMaxHealth;

        // Bereid arrows voor de eerste wave
        PrepareNextWaveArrows();
    }

    void Update()
    {
        // Spatie start de wave pas
        if (Input.GetKeyDown(KeyCode.Space) && !waveActive)
        {
            if (waveNumber <= maxWaves)
                StartCoroutine(SpawnWave());
            else
                Debug.Log("Alle waves zijn al geweest!");
        }
    }

    /// <summary>
    /// Kies random spawnlocaties voor de volgende wave en activeer de bijbehorende arrows
    /// </summary>
    void PrepareNextWaveArrows()
    {
        activeSpawnIndices.Clear();

        int directions = Mathf.Min(waveNumber, spawnPoints.Length); // dag 1 = 1, dag2=2, max 4

        // Kies unieke random indices
        List<int> indicesPool = new List<int> { 0, 1, 2, 3 };
        for (int i = 0; i < directions; i++)
        {
            int randomIndex = Random.Range(0, indicesPool.Count);
            activeSpawnIndices.Add(indicesPool[randomIndex]);
            indicesPool.RemoveAt(randomIndex);
        }

        // Activeer de bijbehorende arrows
        for (int i = 0; i < arrowIndicators.Length; i++)
        {
            if (activeSpawnIndices.Contains(i))
                arrowIndicators[i].GetComponent<ArrowIndicator>().ActivateArrow();
            else
                arrowIndicators[i].GetComponent<ArrowIndicator>().DeactivateArrow();
        }

        Debug.Log($"Dag {waveNumber}: Arrows op spawnpoints {string.Join(",", activeSpawnIndices)}");
    }

    IEnumerator SpawnWave()
    {
        waveActive = true;

        zombiesAlive = Random.Range(4 + (waveNumber - 1) * 2, 8 + (waveNumber - 1) * 2);

        for (int i = 0; i < zombiesAlive; i++)
        {
            // Kies random spawn uit actieve indices
            int spawnIndex = activeSpawnIndices[Random.Range(0, activeSpawnIndices.Count)];
            Transform spawn = spawnPoints[spawnIndex];

            GameObject z = Instantiate(zombiePrefab, spawn.position, Quaternion.identity);
            z.GetComponent<Zombie>().Init(core, this);

            yield return new WaitForSeconds(0.5f);
        }

        // Arrows blijven pulseren tot alle zombies dood zijn
    }

    public void ZombieDied()
    {
        zombiesAlive--;

        if (zombiesAlive <= 0)
        {
            waveActive = false;
            waveNumber++;

            // Zet alle arrows uit
            foreach (GameObject arrow in arrowIndicators)
                arrow.GetComponent<ArrowIndicator>().DeactivateArrow();

            // Bereid arrows voor volgende wave als er nog waves over zijn
            if (waveNumber <= maxWaves)
                PrepareNextWaveArrows();
        }
    }

    public void DamageCore(int damage)
    {
        coreCurrentHealth -= damage;
        coreHealthSlider.value = coreCurrentHealth;

        if (coreCurrentHealth <= 0)
            Debug.Log("Game Over! De speeltuin is gevallen!");
    }
}
