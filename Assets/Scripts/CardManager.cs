using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CardManager : MonoBehaviour
{
    [Header("UI Settings")]
    public RectTransform cardContainer;      // Panel onderaan
    public List<GameObject> cardPrefabs;     // Je 4 verschillende kaartprefabs
    public float hoverOffset = 110f;
    public float spacing = 260f;

    [Header("Gameplay Settings")]
    public int maxCards = 4;

    private List<GameObject> cards = new List<GameObject>();

    void Start()
    {
        // Voeg alle kaarten toe in volgorde
        for (int i = 0; i < Mathf.Min(maxCards, cardPrefabs.Count); i++)
        {
            AddCard(i);
        }
    }

    void AddCard(int prefabIndex)
    {
        if (cards.Count >= maxCards)
        {
            Debug.Log("Max cards reached!");
            return;
        }

        GameObject prefab = cardPrefabs[prefabIndex];

        GameObject newCard = Instantiate(prefab, cardContainer);
        cards.Add(newCard);

        int index = cards.Count - 1;
        RectTransform rt = newCard.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(index * spacing, 0);

        // Hover events toevoegen via EventTrigger
        AddHoverEffect(newCard, rt);
    }

    void AddHoverEffect(GameObject card, RectTransform rt)
    {
        EventTrigger trigger = card.GetComponent<EventTrigger>();
        if (trigger == null) trigger = card.AddComponent<EventTrigger>();

        // Pointer Enter
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((eventData) => { rt.anchoredPosition += new Vector2(0, hoverOffset); });
        trigger.triggers.Add(enterEntry);

        // Pointer Exit
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((eventData) => { rt.anchoredPosition -= new Vector2(0, hoverOffset); });
        trigger.triggers.Add(exitEntry);
    }
}