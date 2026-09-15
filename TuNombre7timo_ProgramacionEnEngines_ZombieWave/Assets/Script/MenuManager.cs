using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    #region Variables

    [SerializeField] GameObject  game, exitGO, loadMenu;
    [SerializeField] Button start, exit, menu;

    #endregion Variables


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