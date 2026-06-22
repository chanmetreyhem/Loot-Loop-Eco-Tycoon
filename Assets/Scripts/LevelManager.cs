using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class LevelManager : MonoBehaviour
{
    [Header("Level Meta Settings")]
    public int currentLevelNumber = 1;
    public int movesRemaining = 25;
    private bool isGameActive = true;

    [Header("Win Conditions (Target Item Counts)")]
    public int targetPlasticCount = 15;
    public int targetMetalCount = 10;

    [HideInInspector] public int currentPlasticCollected = 0;
    [HideInInspector] public int currentMetalCollected = 0;

    [Header("UI Visual Bindings")]
    public Text movesText;
    public Text objectiveText;

    [Header("Game Over Screen Overlays")]
    public GameObject winPanel;
    public GameObject losePanel;

    private ProgressionManager progressionManager;

    void Start()
    {
        progressionManager = FindObjectOfType<ProgressionManager>();

        // Hide Win/Lose panels when scene initialization sequence starts
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        UpdateLevelUI();
    }

    // Called automatically whenever blocks are matched on board
    public void ProcessCollectedItems(string itemTag, int amount)
    {
        if (!isGameActive) return;

        // Track level-specific mission items
        if (itemTag == "Plastic") currentPlasticCollected += amount;
        if (itemTag == "Metal") currentMetalCollected += amount;

        // Deduct 1 move turn usage profile per circle draw sequence
        movesRemaining--;

        UpdateLevelUI();
        CheckGameStatus();
    }

    private void CheckGameStatus()
    {
        // WIN CONDITION: Target collection numbers reached within boundary limits
        if (currentPlasticCollected >= targetPlasticCount && currentMetalCollected >= targetMetalCount)
        {
            WinGame();
        }
        // LOSE CONDITION: Player runs completely out of remaining move turns
        else if (movesRemaining <= 0)
        {
            LoseGame();
        }
    }

    private void WinGame()
    {
        isGameActive = false;
        if (winPanel != null) winPanel.SetActive(true);
        Debug.Log("Level Win Conditions Achieved!");

        // Award bonus system experience towards player account milestones
        if (progressionManager != null)
        {
            progressionManager.AddExperience(500);
        }
    }

    private void LoseGame()
    {
        isGameActive = false;
        if (losePanel != null) losePanel.SetActive(true);
        Debug.Log("Game Over! Remaining player moves depleted.");
    }

    // UI Panel Button Reference to load the next level index layout
    public void LoadNextLevel()
    {
        currentLevelNumber++;
        // Increase target item difficulty slightly for next level setup
        targetPlasticCount += 5;
        targetMetalCount += 3;
        movesRemaining = 25;

        // Reload the scene configuration to regenerate a fresh puzzle grid
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // UI Panel Button Reference to restart current active scene composition
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void UpdateLevelUI()
    {
        if (movesText != null) movesText.text = "Moves: " + movesRemaining.ToString();

        if (objectiveText != null)
        {
            objectiveText.text = $"Collect Target:\n" +
                                 $"Plastic: {currentPlasticCollected}/{targetPlasticCount}\n" +
                                 $"Metal: {currentMetalCollected}/{targetMetalCount}";
        }
    }
}
