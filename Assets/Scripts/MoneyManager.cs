using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    [Header("Money Settings")]
    public int startingMoney = 50;
    public TMP_Text moneyText;

    private int currentMoney;

    void Start()
    {
        currentMoney = startingMoney;
        UpdateMoneyUI();
    }

    public bool SpendMoney(int amount)
    {
        if (amount <= 0) return false;

        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            UpdateMoneyUI();
            return true;
        }
        else
        {
            Debug.Log("Niet genoeg geld!");
            return false;
        }
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0) return;

        currentMoney += amount;
        UpdateMoneyUI();
    }

    void UpdateMoneyUI()
    {
        if (moneyText != null)
            moneyText.text = $"${currentMoney}";
    }

    public int GetCurrentMoney()
    {
        return currentMoney;
    }
}
