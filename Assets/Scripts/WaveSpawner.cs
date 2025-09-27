using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveSpawner : MonoBehaviour
{
    [Header("Zombie Settings")]
    public GameObject zombiePrefab;
    public Transform[] spawnPoints;
    public GameObject[] arrowIndicators;
    public Transform core;

    [Header("Wave Settings")]
    public int waveNumber = 1;
    public int maxWaves = 4; // max dag 4
    private bool waveActive = false;
    private int zombiesAlive;

    private List<int> activeSpawnIndices = new List<int>();

    [Header("Core Health")]
    public Slider coreHealthSlider;
    public int coreMaxHealth = 100;
    private int coreCurrentHealth;

    public MoneyManager moneyManager; // sleep hier de MoneyManager in de inspector
    public int moneyPerZombie = 10;

    public DayManagerTMP_Fade dayManager;

    private bool waveReady = false; // arrows zijn actief, wave kan gestart worden
    public CanvasGroup waveUI;

    void Start()
    {
        coreCurrentHealth = coreMaxHealth;

        if (coreHealthSlider != null)
        {
            coreHealthSlider.maxValue = coreMaxHealth;
            coreHealthSlider.value = coreCurrentHealth;
        }

        if (dayManager != null)
        {
            dayManager.OnDayStarted += PrepareWaveForDay;
        }
    }

    void Update()
    {
        if (waveUI != null)
        {
            waveUI.alpha = waveReady ? 1f : 0f;
            waveUI.interactable = waveReady;
            waveUI.blocksRaycasts = waveReady;
        }
        // Start wave met spatie als arrows al actief zijn
        if (waveReady && !waveActive && Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(SpawnWave());
        }
    }

    private void PrepareWaveForDay(int day)
    {
        waveNumber = day;

        // Kies spawn arrows en activeer ze
        PrepareNextWaveArrows();

        // Zet flag zodat wave kan starten bij spatie
        waveReady = true;

        Debug.Log($"Dag {day} gestart. Druk op Spatie om wave te starten!");
    }

    void PrepareNextWaveArrows()
    {
        activeSpawnIndices.Clear();

        int directions = Mathf.Min(waveNumber, spawnPoints.Length); // dag 1 = 1 arrow, dag 2 = 2, max 4

        List<int> indicesPool = new List<int> { 0, 1, 2, 3 };
        for (int i = 0; i < directions; i++)
        {
            int randomIndex = Random.Range(0, indicesPool.Count);
            activeSpawnIndices.Add(indicesPool[randomIndex]);
            indicesPool.RemoveAt(randomIndex);
        }

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
        waveReady = false; // arrows zijn al actief, nu wave bezig

        zombiesAlive = Random.Range(4 + (waveNumber - 1) * 2, 8 + (waveNumber - 1) * 2);

        for (int i = 0; i < zombiesAlive; i++)
        {
            int spawnIndex = activeSpawnIndices[Random.Range(0, activeSpawnIndices.Count)];
            Transform spawn = spawnPoints[spawnIndex];

            GameObject z = Instantiate(zombiePrefab, spawn.position, Quaternion.identity);
            z.GetComponent<Zombie>().Init(core, this);

            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log($"Wave {waveNumber} gestart met {zombiesAlive} zombies.");
        // arrows blijven actief tot alle zombies dood zijn
    }

    public void ZombieDied()
    {
        zombiesAlive--;

        // Voeg geld toe bij zombie dood
        if (moneyManager != null)
            moneyManager.AddMoney(moneyPerZombie);

        if (zombiesAlive <= 0)
        {
            waveActive = false;

            

            Debug.Log($"Wave {waveNumber} gedaan!");

            // Zeg tegen DayManager dat wave klaar is
            if (dayManager != null)
                dayManager.MarkWaveDone();
        }
    }


    public void DamageCore(int damage)
    {
        coreCurrentHealth -= damage;

        if (coreHealthSlider != null)
            coreHealthSlider.value = coreCurrentHealth;

        if (coreCurrentHealth <= 0)
            Debug.Log("Game Over! De speeltuin is gevallen!");
    }
}
