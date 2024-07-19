using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public GameObject homeScreen;
    public GameObject instructionsScreen;
    public GameObject enterTeamNameScreen;
    public GameObject chooseDifficultyScreen;
    public GameObject gameScreen;
    public GameObject gameEndScreen;
    public GameObject leaderboardScreen;

    public delegate void DifficultySelectedHandler(string difficulty);
    public event DifficultySelectedHandler OnDifficultySelected; // Event for difficulty selection

    void Start()
    {
        ShowHomeScreen();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (homeScreen.activeSelf)
            {
                ShowInstructionsScreen();
            }
            else if (instructionsScreen.activeSelf)
            {
                ShowEnterTeamNameScreen();
            }
        }
    }

    public void ShowHomeScreen()
    {
        homeScreen.SetActive(true);
        instructionsScreen.SetActive(false);
        enterTeamNameScreen.SetActive(false);
        gameScreen.SetActive(false);
        gameEndScreen.SetActive(false);
        leaderboardScreen.SetActive(false);
    }

    public void RestartGame()
    {
        // Restart the whole game by reloading the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ShowInstructionsScreen()
    {
        homeScreen.SetActive(false);
        instructionsScreen.SetActive(true);
        enterTeamNameScreen.SetActive(false);
        chooseDifficultyScreen.SetActive(false);
        gameScreen.SetActive(false);
        gameEndScreen.SetActive(false);
        leaderboardScreen.SetActive(false);
    }

    public void ShowEnterTeamNameScreen()
    {
        homeScreen.SetActive(false);
        instructionsScreen.SetActive(false);
        enterTeamNameScreen.SetActive(true);
        chooseDifficultyScreen.SetActive(false);
        gameScreen.SetActive(false);
        gameEndScreen.SetActive(false);
        leaderboardScreen.SetActive(false);
    }

    public void ShowChooseDifficultyScreen()
    {
        homeScreen.SetActive(false);
        instructionsScreen.SetActive(false);
        enterTeamNameScreen.SetActive(false);
        chooseDifficultyScreen.SetActive(true);
        gameScreen.SetActive(false);
        gameEndScreen.SetActive(false);
        leaderboardScreen.SetActive(false);
    }

    public void ShowGameScreen()
    {
        homeScreen.SetActive(false);
        instructionsScreen.SetActive(false);
        enterTeamNameScreen.SetActive(false);
        chooseDifficultyScreen.SetActive(false);
        gameScreen.SetActive(true);
        gameEndScreen.SetActive(false);
        leaderboardScreen.SetActive(false);
    }

    public void ShowGameEndScreen()
    {
        homeScreen.SetActive(false);
        instructionsScreen.SetActive(false);
        enterTeamNameScreen.SetActive(false);
        chooseDifficultyScreen.SetActive(false);
        gameScreen.SetActive(false);
        gameEndScreen.SetActive(true);
        leaderboardScreen.SetActive(false);
    }

    public void ShowLeaderboardScreen()
    {
        homeScreen.SetActive(false);
        instructionsScreen.SetActive(false);
        enterTeamNameScreen.SetActive(false);
        chooseDifficultyScreen.SetActive(false);
        gameScreen.SetActive(false);
        gameEndScreen.SetActive(false);
        leaderboardScreen.SetActive(true);
    }

    // Method to handle difficulty selection
    public void SelectDifficulty(string difficulty)
    {
        if (OnDifficultySelected != null)
        {
            OnDifficultySelected.Invoke(difficulty);
        }
    }
}
