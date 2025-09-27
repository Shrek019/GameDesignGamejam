using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CardManager : MonoBehaviour
{
    [Header("UI Settings")]
    public RectTransform cardContainer;
    public List<GameObject> cardPrefabs;
    public float hoverOffset = 110f;
    public float spacing = 260f;

    [Header("References")]
    public BuildingManager buildingManager;

    [Header("Gameplay Settings")]
    public int maxCards = 4;

    private List<GameObject> cards = new List<GameObject>();
    private int selectedIndex = -1; // Welke kaart momenteel geselecteerd is

    void Start()
    {
        for (int i = 0; i < Mathf.Min(maxCards, cardPrefabs.Count); i++)
        {
            AddCard(i);
        }
    }

    void Update()
    {
        HandleKeyboardSelection();
    }

    void HandleKeyboardSelection()
    {
        int keyIndex = -1;

        if (Input.GetKeyDown(KeyCode.Alpha1)) keyIndex = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) keyIndex = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) keyIndex = 2;
        if (Input.GetKeyDown(KeyCode.Alpha4)) keyIndex = 3;

        if (keyIndex != -1)
        {
            SelectCard(keyIndex);
            if (buildingManager != null)
                buildingManager.SelectBuildingFromUI(keyIndex);
        }
    }

    void AddCard(int prefabIndex)
    {
        if (cards.Count >= maxCards) return;

        GameObject prefab = cardPrefabs[prefabIndex];
        GameObject newCard = Instantiate(prefab, cardContainer);
        cards.Add(newCard);

        int index = cards.Count - 1;
        RectTransform rt = newCard.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(index * spacing, 0);

        AddHoverEffect(newCard, rt);
        AddCardClick(newCard, prefabIndex, index);
    }

    void AddHoverEffect(GameObject card, RectTransform rt)
    {
        EventTrigger trigger = card.GetComponent<EventTrigger>();
        if (trigger == null) trigger = card.AddComponent<EventTrigger>();

        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((eventData) => { rt.anchoredPosition += new Vector2(0, hoverOffset); });
        trigger.triggers.Add(enterEntry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((eventData) => {
            // Alleen terug naar origineel als het niet geselecteerd is
            int idx = cards.IndexOf(card);
            if (idx != selectedIndex) rt.anchoredPosition -= new Vector2(0, hoverOffset);
        });
        trigger.triggers.Add(exitEntry);
    }

    void AddCardClick(GameObject card, int prefabIndex, int cardIndex)
    {
        Button btn = card.GetComponent<Button>();
        if (btn == null) btn = card.AddComponent<Button>();

        btn.onClick.AddListener(() =>
        {
            SelectCard(cardIndex);
            if (buildingManager != null)
                buildingManager.SelectBuildingFromUI(prefabIndex);
        });
    }

    void SelectCard(int index)
    {
        // Zet alle kaarten terug naar normale positie
        for (int i = 0; i < cards.Count; i++)
        {
            RectTransform rt = cards[i].GetComponent<RectTransform>();
            if (i != index && rt.anchoredPosition.y > 0)
            {
                rt.anchoredPosition -= new Vector2(0, hoverOffset);
            }
        }

        // Zet geselecteerde kaart omhoog
        RectTransform selectedRT = cards[index].GetComponent<RectTransform>();
        if (selectedRT.anchoredPosition.y == 0)
            selectedRT.anchoredPosition += new Vector2(0, hoverOffset);

        selectedIndex = index;
    }
}
