using UnityEngine;
using TMPro;
using UnityEngine.UI; // Make sure to use TextMeshPro for modern, clean UI rendering

public class ScoreSystem : MonoBehaviour
{
    [Header("Currency Profiles")]
    public int ecoCoins = 0;
    public int totalPlasticCollected = 0;
    public int totalMetalCollected = 0;
    public int totalOrganicCollected = 0;

    [Header("UI Visual Bindings")]
    public Text coinText;
    public Text plasticText;
    public Text metalText;
    public Text organicText;

    void Start()
    {
        UpdateUIElements();
    }

    // Processes resource allocation and dynamic calculation balances
    public void AddLootAndCoins(string blockTag, int quantity)
    {
        // Base formula: Players receive 10 standard Eco-Coins per burst object
        int coinReward = quantity * 10;
        ecoCoins += coinReward;

        // Categorize specific inventory items for City building configurations
        switch (blockTag)
        {
            case "Plastic":
                totalPlasticCollected += quantity;
                break;
            case "Metal":
                totalMetalCollected += quantity;
                break;
            case "Organic":
                totalOrganicCollected += quantity;
                break;
        }

        UpdateUIElements();
        Debug.Log("Economy Profile Updated! Current Balance - Coins: " + ecoCoins);
    }

    void UpdateUIElements()
    {
        // Update display text blocks gracefully if references are assigned
        if (coinText != null) coinText.text = "Eco-Coins: " + ecoCoins.ToString();
        if (plasticText != null) plasticText.text = "x" + totalPlasticCollected.ToString();
        if (metalText != null) metalText.text = "x" + totalMetalCollected.ToString();
        if (organicText != null) organicText.text = "x" + totalOrganicCollected.ToString();
    }
}
