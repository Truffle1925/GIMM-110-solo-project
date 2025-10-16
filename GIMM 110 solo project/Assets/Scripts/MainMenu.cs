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

    // Called when Quit button is pressed
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}

