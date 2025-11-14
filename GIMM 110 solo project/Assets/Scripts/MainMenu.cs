using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Called when Play button is pressed
    public void PlayGame()
    {
        // Replace "GameScene" with your actual gameplay scene name
        SceneManager.LoadScene("GameScene");
    }

    public void OpenCredits()
    {
        // Replace "CreditsScene" with your actual credits scene name
        SceneManager.LoadScene("CreditsScene");
    }

    // Called when Quit button is pressed
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    public void MainMenuScene()
    {
        // Replace "MainMenuScene" with your actual main menu scene name
        SceneManager.LoadScene("MainMenu");
    }
}

