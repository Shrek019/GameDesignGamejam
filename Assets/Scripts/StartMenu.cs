using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    // Methode om te koppelen aan de Play Button
    public void PlayGame()
    {
        // Laad SampleScene
        SceneManager.LoadScene("SampleScene");
    }

    // Optioneel: Quit knop
    public void QuitGame()
    {
        Debug.Log("Quit game");
        Application.Quit();
    }
}
