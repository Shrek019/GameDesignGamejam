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

    [Header("References")]
    public BuildingManager buildingManager;

    [Header("Gameplay Settings")]
    public int maxCards = 4;

    private List<GameObject> cards = new List<GameObject>();
    private Dictionary<GameObject, Vector2> originalCardPositions = new Dictionary<GameObject, Vector2>();
    private GameObject selectedCard = null;

    void Start()
    {
        // Voeg alle kaarten toe in volgorde
        for (int i = 0; i < Mathf.Min(maxCards, cardPrefabs.Count); i++)
        {
            AddCard(i);
        }
    }

    void Update()
    {
        HandleCardSelectionWithKeys();
    }

    #region Card Setup
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
        Vector2 basePos = new Vector2(index * spacing, 0);
        rt.anchoredPosition = basePos;

        // Sla originele positie op
        originalCardPositions[newCard] = basePos;

        // Voeg hover effect toe
        AddHoverEffect(newCard, rt);

        // Voeg klik selectie toe
        AddCardClick(newCard, prefabIndex);
    }
    #endregion

    #region Hover Effect
    void AddHoverEffect(GameObject card, RectTransform rt)
    {
        EventTrigger trigger = card.GetComponent<EventTrigger>();
        if (trigger == null) trigger = card.AddComponent<EventTrigger>();

        // Pointer Enter
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((eventData) =>
        {
            if (selectedCard != card)
                rt.anchoredPosition = originalCardPositions[card] + new Vector2(0, hoverOffset);
        });
        trigger.triggers.Add(enterEntry);

        // Pointer Exit
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((eventData) =>
        {
            if (selectedCard != card)
                rt.anchoredPosition = originalCardPositions[card];
        });
        trigger.triggers.Add(exitEntry);
    }
    #endregion

    #region Card Click
    void AddCardClick(GameObject card, int prefabIndex)
    {
        Button btn = card.GetComponent<Button>();
        if (btn == null) btn = card.AddComponent<Button>();

        btn.onClick.AddListener(() =>
        {
            SelectCard(card, prefabIndex);
        });
    }
    #endregion

    #region Keyboard Selection
    void HandleCardSelectionWithKeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectCardByIndex(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectCardByIndex(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectCardByIndex(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectCardByIndex(3);
    }

    void SelectCardByIndex(int index)
    {
        if (index < 0 || index >= cards.Count) return;
        SelectCard(cards[index], index);
    }
    #endregion

    #region Selection Logic
    void SelectCard(GameObject card, int prefabIndex)
    {
        // Reset vorige geselecteerde kaart
        if (selectedCard != null && originalCardPositions.ContainsKey(selectedCard))
            selectedCard.GetComponent<RectTransform>().anchoredPosition = originalCardPositions[selectedCard];

        selectedCard = card;

        // Til de geselecteerde kaart omhoog
        if (originalCardPositions.ContainsKey(card))
            card.GetComponent<RectTransform>().anchoredPosition = originalCardPositions[card] + new Vector2(0, hoverOffset);

        // Selecteer prefab in BuildingManager
        if (buildingManager != null)
            buildingManager.SelectBuildingFromUI(prefabIndex);
    }
    #endregion
}
