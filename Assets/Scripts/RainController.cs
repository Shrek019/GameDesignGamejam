using UnityEngine;
using TMPro; // Alleen nodig als je TextMeshPro gebruikt

public class RainController : MonoBehaviour
{
    [Header("UI Text Element")]
    public TextMeshProUGUI rainText; // sleep hier je TMP Text in
    // Als je normale UI Text gebruikt: public Text rainText;

    [Header("Settings")]
    public float minDelay = 120f; // 2 minuten
    public float maxDelay = 300f; // 5 minuten
    public float displayTime = 5f; // hoeveel seconden het bericht zichtbaar blijft

    [Header("Rain Status")]
    public bool isRaining = false; // houdt bij of het op dit moment regent

    void Start()
    {
        if (rainText != null)
            rainText.gameObject.SetActive(false); // begin met tekst verborgen

        StartCoroutine(RainEventRoutine());
    }

    private System.Collections.IEnumerator RainEventRoutine()
    {
        while (true)
        {
            // Wacht een random tijd tussen 2-5 minuten
            float waitTime = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(waitTime);

            // Start regen
            isRaining = true;

            if (rainText != null)
            {
                rainText.text = "It's raining!";
                rainText.gameObject.SetActive(true);

                // Laat de tekst een paar seconden zien
                yield return new WaitForSeconds(displayTime);

                rainText.gameObject.SetActive(false);
            }

            // Stop regen
            isRaining = false;
        }
    }

    // Functie voor andere scripts om te checken of het regent
    public bool IsRaining()
    {
        return isRaining;
    }
}
