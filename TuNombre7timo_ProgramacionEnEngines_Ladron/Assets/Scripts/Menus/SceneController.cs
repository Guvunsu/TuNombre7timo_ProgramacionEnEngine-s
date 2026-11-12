using UnityEngine;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
  #region Funciones Publicas 
    public void LoadGameplay()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }
    public void LoadMenuPrincipal()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }
    public void LoadLoose()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Loose");
    }
    public void LoadWin()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Win");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    #endregion Funciones Publicas 
}