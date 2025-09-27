using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

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
    public UnityEngine.UI.Slider coreHealthSlider;
    public int coreMaxHealth = 100;
    public int coreCurrentHealth;

    public MoneyManager moneyManager; // sleep hier de MoneyManager in de inspector
    public int moneyPerZombie = 10;

    public DayManagerTMP_Fade dayManager;

    private bool waveReady = false; // arrows zijn actief, wave kan gestart worden
    public CanvasGroup waveUI;

    public CanvasGroup gameOverUI;
    public TextMeshProUGUI gameOverText;
    public AudioSource waveSound;


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

    public void ApplyBuff(string buffName)
    {
        if (buffName == "+30 Playground HP")
        {
            coreMaxHealth += 30;
            coreCurrentHealth += 30;
            if (coreHealthSlider != null)
                coreHealthSlider.value = coreCurrentHealth;
        }

        // hier kan je andere buffs toevoegen:
        else if (buffName == "+10 Extra Money per zombie")
        {
            moneyPerZombie += 10;
        }

        // etc.
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

        zombiesAlive = Random.Range(3 + (waveNumber - 1) * 2, 6 + (waveNumber - 1) * 2);
        waveSound.Play();
        for (int i = 0; i < zombiesAlive; i++)
        {
            int spawnIndex = activeSpawnIndices[Random.Range(0, activeSpawnIndices.Count)];
            Transform spawn = spawnPoints[spawnIndex];

            // Random offset rondom spawn
            float offsetRadius = 2f; // hoe ver ze maximaal kunnen afwijken
            Vector2 randCircle = Random.insideUnitCircle * offsetRadius;
            Vector3 spawnPos = spawn.position + new Vector3(randCircle.x, 0f, randCircle.y);

            GameObject z = Instantiate(zombiePrefab, spawnPos, Quaternion.identity);
            z.GetComponent<Zombie>().Init(core, this);

            yield return new WaitForSeconds(0.5f);
        }
        waveSound.Stop();
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
        {
            Debug.Log("Game Over! De speeltuin is gevallen!");
            if (gameOverUI != null)
                gameOverUI.alpha = 1f;

            int totalScore = 0;

            // Zoek alle gebouwen in de scene met tag "Building"
            GameObject[] buildings = GameObject.FindGameObjectsWithTag("Building");
            foreach (GameObject building in buildings)
            {
                totalScore += 1; // score van dat gebouw
            }

            if (gameOverText != null)
                gameOverText.text = $"Game Over!\nBuilding Score: {totalScore}";

            Time.timeScale = 0f;
            StartCoroutine(GameOverDelay());
        }
    }

    IEnumerator GameOverDelay()
    {
        yield return new WaitForSecondsRealtime(3f); // realtime, want Time.timeScale = 0
        Application.Quit();
    }
}
