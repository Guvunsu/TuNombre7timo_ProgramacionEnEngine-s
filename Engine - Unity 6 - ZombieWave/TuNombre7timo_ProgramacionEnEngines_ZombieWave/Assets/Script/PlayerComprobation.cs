using UnityEngine;
using TMPro;

public class PlayerComprobation : MonoBehaviour
{
    #region Enum
    public enum PlayerLifeState { LIFE, DEAD }
    public PlayerLifeState enumPlayer;
    #endregion

    #region Referencias
    public PlayerMove script_playerMove;
    public PlayerShoot script_playerShoot;
    public SceneManager script_sceneManager;

    [SerializeField] private TextMeshProUGUI livesText;
    #endregion

    #region Variables
    public bool isAlive = true;
    [SerializeField] private float movementDead = 0f;

    [Header("Vidas")]
    [SerializeField] private int currentLives = 3;
    [SerializeField] private int maxLives = 4;
    #endregion

    void Start()
    {
        enumPlayer = PlayerLifeState.LIFE;
        isAlive = true;
        currentLives = 3;
        UpdateLivesText();
    }

    void Update()
    {
        switch (enumPlayer)
        {
            case PlayerLifeState.LIFE:
                PlayerAlive();
                break;
            case PlayerLifeState.DEAD:
                PlayerDead();
                break;
        }
    }

    #region Colisiones
    private void OnCollisionEnter2D(Collision2D collision)
    {
            Debug.Log("1 toco el zombie?");
        if (collision.gameObject.CompareTag("Zombie"))
        {
            Debug.Log("2 toco el zombie?");    
            TakeDamage();
        }
    }
    #endregion

    #region Vidas
    public void TakeDamage()
    {
        if (!isAlive) return;

        currentLives--;
        UpdateLivesText();

        if (currentLives <= 0)
        {
            currentLives = 0;
            enumPlayer = PlayerLifeState.DEAD;
        }
    }

    public void AddLife()
    {
        if (currentLives >= maxLives) return;

        currentLives++;
        UpdateLivesText();
    }

    public int GetCurrentLives()
    {
        return currentLives;
    }

    public int GetMaxLives()
    {
        return maxLives;
    }

    private void UpdateLivesText()
    {
        if (livesText != null)
            livesText.text = currentLives.ToString();
    }
    #endregion

    #region Life/Dead
    public void PlayerAlive()
    {
        isAlive = true;
    }

    public void PlayerDead()
    {
        isAlive = false;
        // Desactivar componentes al morir
        if (script_playerMove != null) script_playerMove.enabled = false;
        if (script_playerShoot != null) script_playerShoot.enabled = false;

        if (script_sceneManager != null)
            script_sceneManager.LoadLoose();
    }
    #endregion
}