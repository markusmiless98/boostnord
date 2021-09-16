using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartGameButton()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitGameButton()
    {
        Application.Quit();
    }
}