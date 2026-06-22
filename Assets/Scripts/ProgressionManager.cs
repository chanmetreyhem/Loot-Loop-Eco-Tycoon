using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class ProgressionManager : MonoBehaviour
{
    [Header("Player Profile")]
    public int playerLevel = 1;
    public int currentEXP = 0;
    public int expRequiredForNextLevel;

    [Header("UI Visual Bindings")]
    public Text profileLevelText;
    public Text profileExpText;

    private ScoreSystem scoreSystem;

    void Start()
    {
        scoreSystem = FindFirstObjectByType<ScoreSystem>();
        CalculateNextLevelRequirements();
        UpdateProgressionUI();
    }

    // Piecewise Exponential Formula for Level 1 - 100 Monetization Strategy
    public void CalculateNextLevelRequirements()
    {
        // Tier 1: Fast Early Onboarding (Levels 1 - 10)
        if (playerLevel <= 10)
        {
            expRequiredForNextLevel = Mathf.RoundToInt(100f * Mathf.Pow(playerLevel, 1.2f));
        }
        // Tier 2: Core Daily Gameplay Retention Habit (Levels 11 - 50)
        else if (playerLevel <= 50)
        {
            expRequiredForNextLevel = Mathf.RoundToInt(120f * Mathf.Pow(playerLevel, 1.5f));
        }
        // Tier 3: Sharp Scale Progression Curve driving Ad Monetization (Levels 51 - 100)
        else
        {
            expRequiredForNextLevel = Mathf.RoundToInt(150f * Mathf.Pow(playerLevel, 1.8f));
        }
    }

    public void AddExperience(int amount)
    {
        currentEXP += amount;

        // Evaluate level progression threshold conditions
        while (currentEXP >= expRequiredForNextLevel)
        {
            currentEXP -= expRequiredForNextLevel;
            playerLevel++;
            Debug.Log("Account Level Up Triggered! Current Tier: " + playerLevel);

            CalculateNextLevelRequirements();
            AwardLevelUpBonuses();
        }
        UpdateProgressionUI();
    }

    private void AwardLevelUpBonuses()
    {
        if (scoreSystem != null)
        {
            // Inject free incentive coins to reward players for completing milestones
            scoreSystem.ecoCoins += playerLevel * 100;
        }
    }

    void UpdateProgressionUI()
    {
        if (profileLevelText != null) profileLevelText.text = "Account Level: " + playerLevel.ToString();
        if (profileExpText != null) profileExpText.text = $"EXP: {currentEXP}/{expRequiredForNextLevel}";
    }
}
