using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    [Header("Zombie Settings")]
    public GameObject zombiePrefab;
    public Transform[] spawnPoints; // 4 pijlen: boven, onder, links, rechts
    public GameObject[] arrowIndicators; // verwijzingen naar de pijlen/indicatoren
    public Transform core;

    [Header("Wave Settings")]
    public int waveNumber = 1;
    private bool waveActive = false;
    public int maxWaves = 5;

    [Header("Core Health")]
    public Slider coreHealthSlider;
    public int coreMaxHealth = 100;
    private int coreCurrentHealth;

    void Start()
    {
        coreCurrentHealth = coreMaxHealth;
        coreHealthSlider.maxValue = coreMaxHealth;
        coreHealthSlider.value = coreMaxHealth;

        // Zet alle pijlen uit bij start
        foreach (GameObject arrow in arrowIndicators)
            arrow.SetActive(false);
    }

    void Update()
    {
        // Spatie = start wave
        if (Input.GetKeyDown(KeyCode.Space) && !waveActive)
        {
            if (waveNumber <= maxWaves)
                StartCoroutine(SpawnWave());
            else
                Debug.Log("Alle waves zijn al geweest!");
        }
    }

    IEnumerator SpawnWave()
    {
        waveActive = true;

        // Bepaal aantal actieve richtingen voor deze wave (dag 1:1, dag2:2,...)
        int activeDirections = Mathf.Min(waveNumber, spawnPoints.Length);

        // Zet pijlen visueel aan
        for (int i = 0; i < arrowIndicators.Length; i++)
        {
            arrowIndicators[i].SetActive(i < activeDirections);
        }

        // aantal zombies = 4 + (waveNumber - 1) * 2 t.e.m. 7 + (waveNumber - 1) * 2
        int zombieCount = Random.Range(4 + (waveNumber - 1) * 2, 8 + (waveNumber - 1) * 2);

        for (int i = 0; i < zombieCount; i++)
        {
            // Kies random spawnpoint uit actieve richtingen
            Transform spawn = spawnPoints[Random.Range(0, activeDirections)];

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
