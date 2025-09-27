using UnityEngine;
using TMPro;
using System.Collections;

public class DayManagerTMP_Fade : MonoBehaviour
{
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

    private void Start()
    {
        if (dayPanel == null || dayText == null)
            Debug.LogError("Assign dayPanel and dayText in inspector!");

        // start met invisible
        dayPanel.alpha = 0f;
        dayPanel.interactable = false;
        dayPanel.blocksRaycasts = false;

        StartCoroutine(DayCycle());
    }

    private IEnumerator DayCycle()
    {
        while (currentDay <= maxDays)
        {
            yield return new WaitForSeconds(dayDuration);

            yield return StartCoroutine(ShowDayPanel(currentDay));

            currentDay++;
        }
    }

    private IEnumerator ShowDayPanel(int day)
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
}