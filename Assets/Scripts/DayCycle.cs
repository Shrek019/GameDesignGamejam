using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DayManagerTMP_Fade : MonoBehaviour
{
    [Header("Other UI Elements")]
    public List<CanvasGroup> otherUI; // sleep hier alle UI die tijdelijk verborgen moet worden

    [Header("UI Elements")]
    public CanvasGroup dayPanel;       // CanvasGroup van het panel
    public TextMeshProUGUI dayText;    // TextMeshPro component

    [Header("Day Settings")]
    public int currentDay = 1;
    public int maxDays = 5;
    public float dayDuration = 120f;    // 2 minuten per dag
    public float panelVisibleTime = 3f; // tijd dat panel volledig zichtbaar blijft

    [Header("Tween Settings")]
    public float fadeTime = 0.5f;       // tijd voor panel fade in/out
    public float textTypeSpeed = 0.2f;  // snelheid van typewriter effect per letter

    [Header("Buff Elements")]
    public GameObject goodCardPrefab;
    public GameObject badCardPrefab;
    public GameObject cardBackPrefab;

    public Transform cardContainer;

    [Header("Card Text Options")]
    public float panelDelay = 0.5f;
    public List<string> buffs;
    public List<string> debuffs;

    private GameObject currentGoodCard;
    private GameObject currentBadCard;

    private Vector2 leftPos = new Vector2(-150f, 0f);
    private Vector2 rightPos = new Vector2(150f, 0f);

    private bool cardsChosen = false;
    public System.Action<int> OnDayStarted;

    // in DayManagerTMP_Fade
    private void Start()
    {
        // arrows moeten eerst hidden zijn
        dayPanel.alpha = 0f;
        dayPanel.interactable = false;
        dayPanel.blocksRaycasts = false;

        // start de dagcyclus
        StartCoroutine(DayCycle());
    }

    public bool IsUICardActive()
    {
        return currentGoodCard != null || currentBadCard != null;
    }

    private void HideOtherUI()
    {
        foreach (var ui in otherUI)
        {
            ui.alpha = 0f;
            ui.interactable = false;
            ui.blocksRaycasts = false;
        }
    }

    private void ShowOtherUI()
    {
        foreach (var ui in otherUI)
        {
            ui.alpha = 1f;
            ui.interactable = true;
            ui.blocksRaycasts = true;
        }
    }

    private IEnumerator DayCycle()
    {
        while (currentDay <= maxDays)
        {
            // 1️⃣ Dag panel tonen
            yield return StartCoroutine(ShowDayPanel(currentDay));

            // 2️⃣ Kaarten kiezen
            cardsChosen = false;
            yield return StartCoroutine(ShowNightCards());
            while (!cardsChosen)
                yield return null;

            // 3️⃣ Wave starten via WaveSpawner (dag = waveNumber)
            waveDone = false; // reset
            OnDayStarted?.Invoke(currentDay);

            // 4️⃣ Wacht tot wave gedaan is (WaveSpawner roept MarkWaveDone aan)
            yield return new WaitUntil(() => waveDone);

            // 5️⃣ Korte pauze en naar volgende dag
            currentDay++;
        }

        Debug.Log("Game voltooid ");
    }



    // dit moet door WaveSpawner op true gezet worden!
    private bool waveDone = false;
    public void MarkWaveDone()
    {
        waveDone = true;
    }

    public IEnumerator ShowDayPanel(int day)
    {
        // Panel fade-in
        yield return StartCoroutine(FadeCanvasGroup(dayPanel, 0f, 1f, fadeTime, true));

        // Typewriter text
        yield return StartCoroutine(TypeText($"Day {day}", textTypeSpeed));

        // Wacht volledig zichtbaar
        yield return new WaitForSeconds(panelVisibleTime);

        // Text fade out
        yield return StartCoroutine(FadeOutText());

        // Panel fade-out
        yield return StartCoroutine(FadeCanvasGroup(dayPanel, 1f, 0f, fadeTime, false));
    }


    

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration, bool interactable)
    {
        float elapsed = 0f;
        cg.interactable = interactable;
        cg.blocksRaycasts = interactable;

        while (elapsed < duration)
        {
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cg.alpha = to;
    }

    private IEnumerator TypeText(string text, float speed)
    {
        dayText.text = "";
        dayText.alpha = 1f;

        foreach (char c in text)
        {
            dayText.text += c;
            yield return new WaitForSeconds(speed);
        }
    }

    private IEnumerator FadeOutText()
    {
        float elapsed = 0f;
        float duration = textTypeSpeed * dayText.text.Length; // gelijk aan type tijd
        float startAlpha = dayText.alpha;

        while (elapsed < duration)
        {
            dayText.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        dayText.alpha = 0f;
        dayText.text = "";
    }

    public IEnumerator ShowNightCards()
    {
        HideOtherUI();
        yield return new WaitForSeconds(panelDelay);

        string goodText = buffs[Random.Range(0, buffs.Count)];
        string badText = debuffs[Random.Range(0, debuffs.Count)];

        // Spawn backcards handmatig
        currentGoodCard = Instantiate(cardBackPrefab, cardContainer);
        currentGoodCard.GetComponent<RectTransform>().anchoredPosition = leftPos;

        currentBadCard = Instantiate(cardBackPrefab, cardContainer);
        currentBadCard.GetComponent<RectTransform>().anchoredPosition = rightPos;

        // Voeg onclick toe
        Button goodBtn = currentGoodCard.GetComponent<Button>();
        goodBtn.onClick.AddListener(() => StartCoroutine(FlipCardManual(true, goodText)));

        Button badBtn = currentBadCard.GetComponent<Button>();
        badBtn.onClick.AddListener(() => StartCoroutine(FlipCardManual(false, badText)));

        // Wacht tot kaarten gekozen zijn
        while (!cardsChosen)
            yield return null;

        ShowOtherUI();
    }

    private IEnumerator FlipCardManual(bool isGood, string cardText)
    {
        GameObject chosenCardBack = isGood ? currentGoodCard : currentBadCard;
        GameObject otherCardBack = isGood ? currentBadCard : currentGoodCard;

        // Spawn gekozen kaart
        GameObject chosenCardPrefab = Instantiate(isGood ? goodCardPrefab : badCardPrefab, cardContainer);
        RectTransform chosenRect = chosenCardPrefab.GetComponent<RectTransform>();
        chosenRect.anchoredPosition = chosenCardBack.GetComponent<RectTransform>().anchoredPosition;

        TMP_Text tmpChosen = chosenCardPrefab.GetComponentInChildren<TMP_Text>();
        if (tmpChosen != null) tmpChosen.text = cardText;

        Destroy(chosenCardBack);
        yield return StartCoroutine(FlipAnimationPrefab(chosenCardPrefab));

        if (cardText == "1 Extra Day")
        {
            maxDays += 1;
        }
        else
        {
            WaveSpawner ws = FindObjectOfType<WaveSpawner>();
            if (ws != null)
            {
                ws.ApplyBuff(cardText); // cardText bevat de naam van de gekozen buff
            }
        }

        // Spawn andere kaart
        GameObject otherCardPrefab = null;
        if (otherCardBack != null)
        {
            otherCardPrefab = Instantiate(isGood ? badCardPrefab : goodCardPrefab, cardContainer);
            RectTransform otherRect = otherCardPrefab.GetComponent<RectTransform>();
            otherRect.anchoredPosition = otherCardBack.GetComponent<RectTransform>().anchoredPosition;

            TMP_Text tmpOther = otherCardPrefab.GetComponentInChildren<TMP_Text>();
            if (tmpOther != null)
                tmpOther.text = isGood ? debuffs[Random.Range(0, debuffs.Count)] : buffs[Random.Range(0, buffs.Count)];

            Destroy(otherCardBack);

            yield return StartCoroutine(FlipAnimationPrefab(otherCardPrefab));
        }

        // Scale animaties
        Vector3 startOther = otherCardPrefab != null ? otherCardPrefab.transform.localScale : Vector3.one;
        Vector3 targetOther = startOther * 0.9f;
        Vector3 startChosen = chosenCardPrefab.transform.localScale;
        Vector3 targetChosen = startChosen * 1.2f;
        float duration = 0.3f;
        float t = 0f;
        while (t < duration)
        {
            chosenCardPrefab.transform.localScale = Vector3.Lerp(startChosen, targetChosen, t / duration);
            if (otherCardPrefab != null)
                otherCardPrefab.transform.localScale = Vector3.Lerp(startOther, targetOther, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        chosenCardPrefab.transform.localScale = targetChosen;
        if (otherCardPrefab != null)
            otherCardPrefab.transform.localScale = targetOther;

        // Wacht 1-2 seconden voordat ze verdwijnen
        yield return new WaitForSeconds(1.5f);

        // Laat de kaarten leuk weggaan (fade + move)
        float fadeDuration = 0.5f;
        t = 0f;
        CanvasGroup chosenCg = chosenCardPrefab.AddComponent<CanvasGroup>();
        CanvasGroup otherCg = null;
        if (otherCardPrefab != null)
            otherCg = otherCardPrefab.AddComponent<CanvasGroup>();

        Vector3 chosenStartPos = chosenCardPrefab.transform.localPosition;
        Vector3 chosenTargetPos = chosenStartPos + new Vector3(0, 100f, 0); // omhoog

        Vector3 otherStartPos = otherCardPrefab != null ? otherCardPrefab.transform.localPosition : Vector3.zero;
        Vector3 otherTargetPos = otherCardPrefab != null ? otherStartPos + new Vector3(0, 100f, 0) : Vector3.zero;

        while (t < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            chosenCg.alpha = alpha;
            chosenCardPrefab.transform.localPosition = Vector3.Lerp(chosenStartPos, chosenTargetPos, t / fadeDuration);

            if (otherCardPrefab != null)
            {
                otherCg.alpha = alpha;
                otherCardPrefab.transform.localPosition = Vector3.Lerp(otherStartPos, otherTargetPos, t / fadeDuration);
            }

            t += Time.deltaTime;
            yield return null;
        }

        // Zorg dat ze echt weg zijn
        Destroy(chosenCardPrefab);
        if (otherCardPrefab != null)
            Destroy(otherCardPrefab);

        // Nu mag de dagcyclus verder
        cardsChosen = true;

        CameraController camCtrl = FindObjectOfType<CameraController>();
        if (camCtrl != null)
            camCtrl.ResetToStartPosition();

        currentGoodCard = null;
        currentBadCard = null;
    }


    private IEnumerator FlipAnimationPrefab(GameObject card)
    {
        float duration = 0.3f;
        float elapsed = 0f;
        Quaternion startRot = card.transform.rotation;
        Quaternion midRot = Quaternion.Euler(0, 90f, 0);
        Quaternion endRot = Quaternion.Euler(0, 0, 0);

        while (elapsed < duration)
        {
            card.transform.rotation = Quaternion.Slerp(startRot, midRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        card.transform.rotation = midRot;

        elapsed = 0f;
        while (elapsed < duration)
        {
            card.transform.rotation = Quaternion.Slerp(midRot, endRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        card.transform.rotation = endRot;
    }


    private void ChooseCard(bool isGood)
    {
        if (isGood)
            Debug.Log("Speler koos de GOOD card!");
        else
            Debug.Log("Speler koos de BAD card!");

        // Verwijder beide kaarten na keuze
        if (currentGoodCard != null) Destroy(currentGoodCard);
        if (currentBadCard != null) Destroy(currentBadCard);
    }
}